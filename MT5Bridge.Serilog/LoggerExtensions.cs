using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Serilog;
using Serilog.Events;

namespace MT5Bridge.Serilog;

/// <summary>
/// Extension methods for Serilog.ILogger to provide high-performance model logging.
/// </summary>
public static class LoggerExtensions
{
    /// <summary>
    /// Gets or sets the default JSON serialization context used when no specific context or type info is provided.
    /// </summary>
    public static JsonSerializerContext? DefaultContext { get; set; }

    /// <summary>
    /// Attaches a model using a JsonSerializerContext to resolve the type info.
    /// </summary>
    public static ILogger WithModel<T>(this ILogger logger, string propertyName, T model, JsonSerializerContext context)
    {
        var typeInfo = (JsonTypeInfo<T>?)context.GetTypeInfo(typeof(T))
            ?? throw new InvalidOperationException($"Type {typeof(T).Name} is not registered in the provided JsonContext");
        return logger.WithModel(propertyName, model, typeInfo);
    }

    /// <summary>
    /// Attaches a model to the log context using Source-Generated JSON for high performance.
    /// Note: This method performs eager evaluation (always serializes the model regardless of log level).
    /// For lazy evaluation (skip serialization if log level is not enabled), use WithModelLazy() instead.
    /// Returns a new ILogger instance with the context attached (Fluent API).
    /// </summary>
    public static ILogger WithModel<T>(this ILogger logger, string propertyName, T model, JsonTypeInfo<T> typeInfo)
    {
        if (model == null) return logger;

        try
        {
            var props = ModelToDictionary(model, typeInfo);
            return logger.ForContext(propertyName, props, destructureObjects: true);
        }
        catch (Exception ex)
        {
            // If serialization fails, we log the error but return the original logger to avoid breaking the chain
            logger.Error(ex, "Failed to serialize model {PropertyName} for logging", propertyName);
            return logger;
        }
    }

    /// <summary>
    /// Attaches a model to the log context with lazy evaluation using a JsonSerializerContext.
    /// </summary>
    public static ILogger WithModelLazy<T>(
        this ILogger logger,
        string propertyName,
        T model,
        JsonSerializerContext context,
        LogEventLevel level = LogEventLevel.Information)
    {
        if (!logger.IsEnabled(level)) return logger;
        var typeInfo = (JsonTypeInfo<T>?)context.GetTypeInfo(typeof(T))
            ?? throw new InvalidOperationException($"Type {typeof(T).Name} is not registered in the provided JsonContext");
        return logger.WithModelLazy(propertyName, model, typeInfo, level);
    }

    /// <summary>
    /// Attaches a model to the log context with lazy evaluation (deferred serialization).
    /// The model is only serialized if the specified log level is enabled, avoiding unnecessary computation.
    /// Recommended for high-frequency logging or expensive model serialization.
    /// Returns a new ILogger instance with the context attached (Fluent API).
    /// </summary>
    public static ILogger WithModelLazy<T>(
        this ILogger logger,
        string propertyName,
        T model,
        JsonTypeInfo<T> typeInfo,
        LogEventLevel level = LogEventLevel.Information)
    {
        if (model == null) return logger;

        // Check if the log level is enabled before performing expensive serialization
        if (!logger.IsEnabled(level))
            return logger;

        try
        {
            var props = ModelToDictionary(model, typeInfo);
            return logger.ForContext(propertyName, props, destructureObjects: true);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Failed to serialize model {PropertyName} for logging", propertyName);
            return logger;
        }
    }

    /// <summary>
    /// Attaches a model directly as a JSON string using a JsonSerializerContext.
    /// </summary>
    public static ILogger WithModelDirect<T>(this ILogger logger, string propertyName, T model, JsonSerializerContext context)
    {
        var typeInfo = (JsonTypeInfo<T>?)context.GetTypeInfo(typeof(T))
            ?? throw new InvalidOperationException($"Type {typeof(T).Name} is not registered in the provided JsonContext");
        return logger.WithModelDirect(propertyName, model, typeInfo);
    }

    /// <summary>
    /// Attaches a model directly as a JSON string (high-performance variant).
    /// Avoids intermediate Dictionary allocation and boxing. Recommended for high-frequency logging.
    /// Note: This method performs eager evaluation (always serializes).
    /// For lazy evaluation, use WithModelDirectLazy() instead.
    /// Returns a new ILogger instance with the context attached (Fluent API).
    /// </summary>
    public static ILogger WithModelDirect<T>(this ILogger logger, string propertyName, T model, JsonTypeInfo<T> typeInfo)
    {
        if (model == null) return logger;

        try
        {
            var json = JsonSerializer.Serialize(model, typeInfo);
            return logger.ForContext(propertyName, json, destructureObjects: false);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Failed to serialize model {PropertyName} for logging", propertyName);
            return logger;
        }
    }

    /// <summary>
    /// Attaches a model directly as a JSON string with lazy evaluation using a JsonSerializerContext.
    /// </summary>
    public static ILogger WithModelDirectLazy<T>(
        this ILogger logger,
        string propertyName,
        T model,
        JsonSerializerContext context,
        LogEventLevel level = LogEventLevel.Information)
    {
        if (!logger.IsEnabled(level)) return logger;
        var typeInfo = (JsonTypeInfo<T>?)context.GetTypeInfo(typeof(T))
            ?? throw new InvalidOperationException($"Type {typeof(T).Name} is not registered in the provided JsonContext");
        return logger.WithModelDirectLazy(propertyName, model, typeInfo, level);
    }

    /// <summary>
    /// Attaches a model directly as a JSON string with lazy evaluation (deferred serialization).
    /// Highest performance option: skips serialization if log level is not enabled AND avoids Dictionary allocation.
    /// Recommended for high-frequency logging (10,000+/sec) with expensive model serialization.
    /// Returns a new ILogger instance with the context attached (Fluent API).
    /// </summary>
    public static ILogger WithModelDirectLazy<T>(
        this ILogger logger,
        string propertyName,
        T model,
        JsonTypeInfo<T> typeInfo,
        LogEventLevel level = LogEventLevel.Information)
    {
        if (model == null) return logger;

        // Check if the log level is enabled before performing expensive serialization
        if (!logger.IsEnabled(level))
            return logger;

        try
        {
            var json = JsonSerializer.Serialize(model, typeInfo);
            return logger.ForContext(propertyName, json, destructureObjects: false);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Failed to serialize model {PropertyName} for logging", propertyName);
            return logger;
        }
    }

    /// <summary>
    /// Writes a log message with a model using Source-Generated JSON context for high performance.
    /// Note: This method always serializes regardless of log level (eager evaluation).
    /// For lazy evaluation, check IsEnabled() before calling, or use LogModelLazy() instead.
    /// </summary>
    public static void LogModel<T>(this ILogger logger, LogEventLevel level, string message, T model, JsonTypeInfo<T> typeInfo, Exception? exception = null)
    {
        logger.WithModel("Model", model, typeInfo).Write(level, exception, message);
    }

    /// <summary>
    /// Writes a log message with a model using lazy evaluation (deferred serialization).
    /// The model is only serialized if the log level is enabled.
    /// Recommended for high-frequency logging with expensive model serialization.
    /// </summary>
    public static void LogModelLazy<T>(this ILogger logger, LogEventLevel level, string message, T model, JsonTypeInfo<T> typeInfo, Exception? exception = null)
    {
        if (!logger.IsEnabled(level))
            return;

        logger.WithModel("Model", model, typeInfo).Write(level, exception, message);
    }

    internal static Dictionary<string, object> ModelToDictionary<T>(T model, JsonTypeInfo<T> typeInfo)
    {
        using var doc = JsonSerializer.SerializeToDocument(model, typeInfo);
        var props = new Dictionary<string, object>();

        foreach (var prop in doc.RootElement.EnumerateObject())
        {
            props[prop.Name] = MapElement(prop.Value) ?? "null";
        }
        return props;
    }

    private static object? MapElement(JsonElement element) => element.ValueKind switch
    {
        JsonValueKind.Object => element.EnumerateObject().ToDictionary(p => p.Name, p => MapElement(p.Value)),
        JsonValueKind.Array => element.EnumerateArray().Select(MapElement).ToList(),
        JsonValueKind.String => element.GetString(),
        JsonValueKind.Number => element.GetDouble(),
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        JsonValueKind.Null => null,
        _ => element.GetRawText()
    };

    // Convenience aliases for common levels (with lazy evaluation)
    /// <summary>
    /// Logs a model at Information level with lazy evaluation.
    /// Only serializes if Information level is enabled.
    /// </summary>
    public static void LogModelInfo<T>(this ILogger logger, string message, T model, JsonTypeInfo<T> typeInfo)
    {
        if (!logger.IsEnabled(LogEventLevel.Information))
            return;
        logger.LogModel(LogEventLevel.Information, message, model, typeInfo);
    }

    /// <summary>
    /// Logs a model at Debug level with lazy evaluation.
    /// Only serializes if Debug level is enabled.
    /// </summary>
    public static void LogModelDebug<T>(this ILogger logger, string message, T model, JsonTypeInfo<T> typeInfo)
    {
        if (!logger.IsEnabled(LogEventLevel.Debug))
            return;
        logger.LogModel(LogEventLevel.Debug, message, model, typeInfo);
    }

    /// <summary>
    /// Logs a model at Error level with lazy evaluation.
    /// Only serializes if Error level is enabled.
    /// </summary>
    public static void LogModelError<T>(this ILogger logger, string message, T model, JsonTypeInfo<T> typeInfo, Exception? ex = null)
    {
        if (!logger.IsEnabled(LogEventLevel.Error))
            return;
        logger.LogModel(LogEventLevel.Error, message, model, typeInfo, ex);
    }

    /// <summary>
    /// Attaches a simple field to the log context (Zap-like syntax).
    /// Returns a new ILogger instance with the context attached.
    /// </summary>
    public static ILogger WithField(this ILogger logger, string name, object value)
    {
        return logger.ForContext(name, value);
    }
}

/// <summary>
/// High-performance model logger wrapper that binds ILogger with a configurable LogEventLevel.
/// Reduces parameter passing by storing log level as an instance property.
/// Supports fluent API for method chaining.
/// </summary>
public sealed class ModelLogger
{
    private ILogger _logger;
    private LogEventLevel _level;
    private JsonSerializerContext? _context;

    /// <summary>
    /// Creates a new ModelLogger instance with specified log level and optional JSON context.
    /// </summary>
    public ModelLogger(ILogger logger, LogEventLevel level = LogEventLevel.Information, JsonSerializerContext? context = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _level = level;
        _context = context ?? LoggerExtensions.DefaultContext;
    }

    /// <summary>
    /// Gets or sets the default log level for this logger.
    /// Changing this affects all subsequent logging operations.
    /// </summary>
    public LogEventLevel Level
    {
        get => _level;
        set => _level = value;
    }

    /// <summary>
    /// Gets or sets the JSON serialization context for this logger.
    /// </summary>
    public JsonSerializerContext? Context
    {
        get => _context;
        set => _context = value;
    }

    /// <summary>
    /// Attaches a model using the logger's configured context.
    /// </summary>
    public ModelLogger WithModel<T>(string propertyName, T model)
    {
        if (_context == null)
            throw new InvalidOperationException("JsonContext must be provided via constructor, property, or LoggerExtensions.DefaultContext");

        var typeInfo = (JsonTypeInfo<T>?)_context.GetTypeInfo(typeof(T))
            ?? throw new InvalidOperationException($"Type {typeof(T).Name} is not registered in the configured JsonContext");

        return WithModel(propertyName, model, typeInfo);
    }

    /// <summary>
    /// Attaches a model with lazy evaluation (deferred serialization).
    /// Model is only serialized if the current log level is enabled.
    /// Returns self for fluent API chaining.
    /// </summary>
    public ModelLogger WithModel<T>(string propertyName, T model, JsonTypeInfo<T> typeInfo)
    {
        if (model == null) return this;

        if (!_logger.IsEnabled(_level))
            return this;

        try
        {
            var props = LoggerExtensions.ModelToDictionary(model, typeInfo);
            _logger = _logger.ForContext(propertyName, props, destructureObjects: true);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to serialize model {PropertyName} for logging", propertyName);
        }

        return this;
    }

    /// <summary>
    /// Attaches a model directly as JSON using the logger's configured context.
    /// </summary>
    public ModelLogger WithModelDirect<T>(string propertyName, T model)
    {
        if (_context == null)
            throw new InvalidOperationException("JsonContext must be provided via constructor, property, or LoggerExtensions.DefaultContext");

        var typeInfo = (JsonTypeInfo<T>?)_context.GetTypeInfo(typeof(T))
            ?? throw new InvalidOperationException($"Type {typeof(T).Name} is not registered in the configured JsonContext");

        return WithModelDirect(propertyName, model, typeInfo);
    }

    /// <summary>
    /// Attaches a model directly as JSON (high-performance variant without intermediate Dictionary).
    /// Model is only serialized if the current log level is enabled.
    /// Returns self for fluent API chaining.
    /// </summary>
    public ModelLogger WithModelDirect<T>(string propertyName, T model, JsonTypeInfo<T> typeInfo)
    {
        if (model == null) return this;

        if (!_logger.IsEnabled(_level))
            return this;

        try
        {
            var json = JsonSerializer.Serialize(model, typeInfo);
            _logger = _logger.ForContext(propertyName, json, destructureObjects: false);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to serialize model {PropertyName} for logging", propertyName);
        }

        return this;
    }

    /// <summary>
    /// Attaches a simple field to the log context.
    /// Returns self for fluent API chaining.
    /// </summary>
    public ModelLogger WithField(string name, object value)
    {
        _logger = _logger.ForContext(name, value);
        return this;
    }

    /// <summary>
    /// Temporarily changes the log level for the next Write() call.
    /// Returns self for fluent API chaining.
    /// </summary>
    public ModelLogger WithLevel(LogEventLevel level)
    {
        _level = level;
        return this;
    }

    /// <summary>
    /// Sets the JSON serialization context for this logger.
    /// Returns self for fluent API chaining.
    /// </summary>
    public ModelLogger WithContext(JsonSerializerContext context)
    {
        _context = context;
        return this;
    }

    /// <summary>
    /// Writes the log message at the configured level.
    /// </summary>
    public void Write(string message)
    {
        if (_logger.IsEnabled(_level))
            _logger.Write(_level, message);
    }

    /// <summary>
    /// Writes the log message with exception at the configured level.
    /// </summary>
    public void Write(string message, Exception? exception)
    {
        if (_logger.IsEnabled(_level))
            _logger.Write(_level, exception, message);
    }

    /// <summary>
    /// Convenience method to write at Information level.
    /// </summary>
    public void WriteInfo(string message)
    {
        if (_logger.IsEnabled(LogEventLevel.Information))
            _logger.Information(message);
    }

    /// <summary>
    /// Convenience method to write at Debug level.
    /// </summary>
    public void WriteDebug(string message)
    {
        if (_logger.IsEnabled(LogEventLevel.Debug))
            _logger.Debug(message);
    }

    /// <summary>
    /// Convenience method to write at Warning level.
    /// </summary>
    public void WriteWarning(string message)
    {
        if (_logger.IsEnabled(LogEventLevel.Warning))
            _logger.Warning(message);
    }

    /// <summary>
    /// Convenience method to write at Error level.
    /// </summary>
    public void WriteError(string message, Exception? ex = null)
    {
        if (_logger.IsEnabled(LogEventLevel.Error))
            _logger.Error(ex, message);
    }

    /// <summary>
    /// Convenience method to write at Fatal level.
    /// </summary>
    public void WriteFatal(string message, Exception? ex = null)
    {
        if (_logger.IsEnabled(LogEventLevel.Fatal))
            _logger.Fatal(ex, message);
    }
}
