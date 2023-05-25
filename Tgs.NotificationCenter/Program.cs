using FluentdForward.OpenTelemetry.MessagePack;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Tgs.Notification.MessageQueue;
using Tgs.Notification.Repositories.DependencyInjection;
using Tgs.NotificationCenter;

var builder = WebApplication.CreateBuilder(args);

builder.Logging
	.AddOpenTelemetry(options =>
	{
		options.IncludeFormattedMessage = true;
		options.ParseStateValues = true;
		_ = options.AddFluentdForwardExporter(fluentOptions =>
		{
			var nodeIp = builder.Configuration.GetValue<string>("LogServiceDomain")!;
			fluentOptions.Host = nodeIp;
			fluentOptions.Tag = builder.Configuration.GetValue<string>("OTEL_SERVICE_NAME")!;
			fluentOptions.UseMessagePack(LogRecordFormatterResolver.GetResolverInstanceWithExtendInfo(
				"MachineInfo",
				() => new
				{
					Environment.MachineName
				}));
		});
	});

builder.Services
	.AddSingleton(sp => new RedisConnectionManager(
		builder.Configuration.GetConnectionString("Redis")!))
	.AddNotificationRepositories(sp => sp.GetRequiredService<RedisConnectionManager>().Database)
	.AddMessageQueue()
	.AddNatsMessageQueue(natsConfig => natsConfig
		.ConfigQueueOptions((options, _) => options.Url =
			builder.Configuration.GetConnectionString("Nats")!)
		.AddHandler(
			ProcessorTypes.PushNotificationHandlerType,
			"tgs.notify.push",
			"tgs.notify.push")
		.AddHandler(
			ProcessorTypes.NotificationAcceptedHandlerType,
			"tgs.notify.accepted",
			"tgs.notify.accepted")
		.AddHandler(
			ProcessorTypes.ResendNotificationHandlerType,
			"tgs.notify.resend",
			"tgs.notify.resend"))
	.AddNatsGlobPatternExchange("*");

builder.Services
	.AddHealthChecks()
	.Services
	.AddOpenTelemetry()
	.WithTracing(traceBuilder => traceBuilder
		.SetResourceBuilder(ResourceBuilder.CreateDefault()
			.AddEnvironmentVariableDetector())
		.AddSource("Tgs.*")
		.AddAspNetCoreInstrumentation()
		.AddOtlpExporter())
	.WithMetrics(meterBuiler => meterBuiler
		.AddRuntimeInstrumentation()
		.AddAspNetCoreInstrumentation()
		.AddOtlpExporter())
	.Services
	.Configure<AspNetCoreInstrumentationOptions>(options => options.Filter =
		httpContext => !httpContext.Request.Path.HasValue
			|| !httpContext.Request.Path.StartsWithSegments("/swagger")
			&& !httpContext.Request.Path.StartsWithSegments("/healthz")
			&& !httpContext.Request.Path.StartsWithSegments("/metrics")
			&& !new[] { "html", "ico", "png", "jpg" }.Any(
				ext => httpContext.Request.Path.Value.EndsWith($".{ext}")));

var app = builder.Build();

app.MapHealthChecks("/healthz");

app.Run();
