using FluentdForward.OpenTelemetry.MessagePack;
using Microsoft.AspNetCore.Mvc;
using NATS.Client.Core;
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
	.AddRouting(routeOpt => routeOpt.LowercaseUrls = true)
	.AddControllers()
	.AddMvcOptions(mvcOptions => mvcOptions.AddWebApiToolkitFilters());

builder.Services
	.AddSingleton(sp => new RedisConnectionManager(
		builder.Configuration.GetConnectionString("Redis")!))
	.AddNotificationRepositories(sp => sp.GetRequiredService<RedisConnectionManager>().Database)
	.AddKeyedSingleton(
		"Nats",
		(sp, _) => new NatsConnection(NatsOpts.Default with
		{
			Url = builder.Configuration.GetConnectionString("Nats")!
		}))
	.AddMessageQueue()
	.AddNatsMessageQueue(natsConfig => natsConfig
		.ConfigureResolveConnection(sp => sp.GetRequiredKeyedService<NatsConnection>("Nats"))
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
	.AddSwaggerGen(options =>
	{
		options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
		{
			Version = "v1",
			Title = "Notification Center Api"
		});

		options.ApplyWebApiToolkitSupport();

		var xmlFileNames = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.xml");
		foreach (var xmlFileName in xmlFileNames)
			options.IncludeXmlComments(xmlFileName);
	});

var excludeExtensions = new[] { "html", "ico", "png", "jpg" };
builder.Services
	.AddHealthChecks()
	.Services
	.AddOpenTelemetry()
	.WithTracing(traceBuilder => traceBuilder
		.SetResourceBuilder(ResourceBuilder.CreateDefault()
			.AddEnvironmentVariableDetector())
		.AddSource("Tgs.*")
		.AddSource("Valhalla.*")
		.AddAspNetCoreInstrumentation(options => options.Filter =
			httpContext => !httpContext.Request.Path.HasValue
				|| !httpContext.Request.Path.StartsWithSegments("/swagger")
				&& !httpContext.Request.Path.StartsWithSegments("/healthz")
				&& !httpContext.Request.Path.StartsWithSegments("/metrics")
				&& !excludeExtensions.Any(
					ext => httpContext.Request.Path.Value.EndsWith($".{ext}")))
		.AddOtlpExporter())
	.WithMetrics(meterBuiler => meterBuiler
		.AddRuntimeInstrumentation()
		.AddAspNetCoreInstrumentation()
		.AddOtlpExporter());

var app = builder.Build();

app.MapControllers();
app.UseSwagger();
app.UseSwaggerUI();
app.MapHealthChecks("/healthz");

app.Run();
