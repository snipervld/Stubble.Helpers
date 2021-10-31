using System;
using System.Collections.Immutable;
using System.Globalization;

namespace Stubble.Helpers
{
    internal class HelperExecutor
    {
        private readonly ImmutableDictionary<string, HelperRef> _helperCache;

        public HelperExecutor(ImmutableDictionary<string, HelperRef> helperCache)
        {
            _helperCache = helperCache;
        }

        public bool TryExecuteHelper(IHelperCallInfo callInfo, HelperContext helperContext, out object value)
        {
            if (callInfo == null)
            {
                throw new ArgumentNullException(nameof(callInfo));
            }

            value = null;

            if (!string.IsNullOrEmpty(callInfo.Identifier)
                && _helperCache.TryGetValue(callInfo.Identifier, out var helper))
            {
                var args = callInfo.Args;
                var argumentTypes = helper.ArgumentTypes;
                var cultureInfo = helperContext.RenderSettings.CultureInfo;

                if (argumentTypes.Length - 1 == args.Length)
                {
                    var arr = new object[args.Length + 1];
                    arr[0] = helperContext;

                    for (var i = 0; i < args.Length; i++)
                    {
                        var arg =
                            args[i].ShouldAttemptContextLoad
                                ? helperContext.Lookup(args[i].Value)
                                : args[i].Value;

                        arg = TryConvertTypeIfRequired(arg, args[i].Value, argumentTypes[i + 1], cultureInfo);

                        if (arg is null)
                        {
                            return false;
                        }

                        arr[i + 1] = arg;
                    }

                    value = helper.Delegate.Method.Invoke(helper.Delegate.Target, arr);
                }

                return true;
            }

            return false;
        }

        private static object TryConvertTypeIfRequired(object value, string arg, Type type, CultureInfo cultureInfo = null)
        {
            if (value is null && !type.IsValueType)
            {
                return null;
            }
            else if (value is null)
            {
                // When lookup is null and type is not a string we should try convert since may be a constant integer or float.
                value = arg;
            }

            var lookupType = value.GetType();

            if (lookupType == type)
            {
                return value;
            }

            if (type.IsAssignableFrom(lookupType))
            {
                return value;
            }

            try
            {
                return Convert.ChangeType(value, type, cultureInfo ?? CultureInfo.InvariantCulture);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch
#pragma warning restore CA1031 // Do not catch general exception types
            {
            }

            return null;
        }
    }
}
