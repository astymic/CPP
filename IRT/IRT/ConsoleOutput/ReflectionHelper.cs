using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace IRT.ConsoleOutput
{
    public static class ReflectionHelper
    {
        public static TValue? GetPropertyValue<TValue>(object? item, string propertyName)
        {
            if (item == null) return default;
            PropertyInfo? property = item.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (property != null && property.CanRead)
            {
                try
                {
                    object? value = property.GetValue(item);
                    if (value == null) return default;

                    if (value is TValue correctlyTypedValue) return correctlyTypedValue;

                    Type tValueType = typeof(TValue);
                    Type? underlyingTValueType = Nullable.GetUnderlyingType(tValueType);

                    if (underlyingTValueType != null)
                    {
                        if (value.GetType() == underlyingTValueType)
                        {
                            return (TValue)Convert.ChangeType(value, underlyingTValueType, CultureInfo.InvariantCulture);
                        }
                    }

                    try
                    {
                        return (TValue)Convert.ChangeType(value, underlyingTValueType ?? tValueType, CultureInfo.InvariantCulture);
                    }
                    catch (InvalidCastException)
                    {
                        if (tValueType == typeof(string))
                            return (TValue)(object)value.ToString()!;

                        System.Diagnostics.Debug.WriteLine($"Reflection: Could not cast/convert value of type {value.GetType().Name} to {tValueType.Name} for property '{propertyName}'.");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Reflection Error getting '{propertyName}': {ex.GetType().Name} - {ex.Message}");
                }
            }
            return default;
        }

        public static bool TryParseAndSetValue(object itemToModify, PropertyInfo selectedProperty, string newValueString)
        {
            Type propertyType = selectedProperty.PropertyType;
            Type underlyingType = Nullable.GetUnderlyingType(propertyType);
            bool isNullable = underlyingType != null;
            Type targetType = underlyingType ?? propertyType;
            object? convertedValue;

            if (isNullable && string.IsNullOrEmpty(newValueString))
            {
                convertedValue = null;
            }
            else
            {
                try
                {
                    if (targetType == typeof(bool))
                    {
                        string lowerVal = newValueString.Trim().ToLowerInvariant();
                        if (lowerVal == "true" || lowerVal == "1" || lowerVal == "yes" || lowerVal == "y")
                            convertedValue = true;
                        else if (lowerVal == "false" || lowerVal == "0" || lowerVal == "no" || lowerVal == "n")
                            convertedValue = false;
                        else
                            throw new FormatException($"Cannot convert '{newValueString}' to Boolean.");
                    }
                    else
                    {
                        TypeConverter converter = TypeDescriptor.GetConverter(targetType);
                        if (converter != null && converter.CanConvertFrom(typeof(string)))
                        {
                            convertedValue = converter.ConvertFromString(null, CultureInfo.InvariantCulture, newValueString);
                        }
                        else
                        {
                            convertedValue = Convert.ChangeType(newValueString, targetType, CultureInfo.InvariantCulture);
                        }
                    }
                }
                catch (Exception ex) when (ex is FormatException || ex is InvalidCastException || ex is NotSupportedException || ex is ArgumentException)
                {
                    ConsoleUI.PrintErrorMessage($"Conversion Error: Could not convert '{newValueString}' to type {targetType.Name}. {ex.Message}");
                    return false;
                }
            }

            try
            {
                selectedProperty.SetValue(itemToModify, convertedValue, null);
                return true;
            }
            catch (TargetInvocationException tie)
            {
                ConsoleUI.PrintErrorMessage($"Validation Error setting property '{selectedProperty.Name}': {tie.InnerException?.Message ?? tie.Message}");
            }
            catch (ArgumentException argEx)
            {
                ConsoleUI.PrintErrorMessage($"Error setting property '{selectedProperty.Name}': Type mismatch or invalid argument. {argEx.Message}");
            }
            catch (Exception ex)
            {
                ConsoleUI.PrintErrorMessage($"Unexpected error setting property '{selectedProperty.Name}': {ex.Message}");
            }
            return false;
        }
    }
}