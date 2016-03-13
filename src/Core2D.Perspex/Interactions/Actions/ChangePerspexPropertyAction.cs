﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Perspex;
using Perspex.Xaml.Interactions.Core;
using Perspex.Xaml.Interactivity;
using System;
using System.Globalization;
using System.Reflection;

namespace Core2D.Perspex.Interactions.Actions
{
    /// <summary>
    /// An action that will change a specified perspex property to a specified value when invoked.
    /// </summary>
    public sealed class ChangePerspexPropertyAction : PerspexObject, IAction
    {
        /// <summary>
        /// Identifies the <seealso cref="PropertyName"/> perspex property.
        /// </summary>
        public static readonly PerspexProperty PropertyNameProperty =
            PerspexProperty.Register<ChangePerspexPropertyAction, PerspexProperty>("PropertyName");

        /// <summary>
        /// Identifies the <seealso cref="TargetObject"/> perspex property.
        /// </summary>
        public static readonly PerspexProperty TargetObjectProperty =
            PerspexProperty.Register<ChangePerspexPropertyAction, object>("TargetObject");

        /// <summary>
        /// Identifies the <seealso cref="Value"/> perspex property.
        /// </summary>
        public static readonly PerspexProperty ValueProperty =
            PerspexProperty.Register<ChangePerspexPropertyAction, object>("Value");

        /// <summary>
        /// Gets or sets the name of the property to change. This is a perspex property.
        /// </summary>
        public PerspexProperty PropertyName
        {
            get { return (PerspexProperty)this.GetValue(ChangePerspexPropertyAction.PropertyNameProperty); }
            set { this.SetValue(ChangePerspexPropertyAction.PropertyNameProperty, value); }
        }

        /// <summary>
        /// Gets or sets the value to set. This is a perspex property.
        /// </summary>
        public object Value
        {
            get { return this.GetValue(ChangePerspexPropertyAction.ValueProperty); }
            set { this.SetValue(ChangePerspexPropertyAction.ValueProperty, value); }
        }

        /// <summary>
        /// Gets or sets the object whose property will be changed.
        /// If <seealso cref="TargetObject"/> is not set or cannot be resolved, the sender of <seealso cref="Execute"/> will be used. This is a perspex property.
        /// </summary>
        public object TargetObject
        {
            get { return (object)this.GetValue(ChangePerspexPropertyAction.TargetObjectProperty); }
            set { this.SetValue(ChangePerspexPropertyAction.TargetObjectProperty, value); }
        }

        /// <summary>
        /// Executes the action.
        /// </summary>
        /// <param name="sender">The <see cref="System.Object"/> that is passed to the action by the behavior. Generally this is <seealso cref="IBehavior.AssociatedObject"/> or a target object.</param>
        /// <param name="parameter">The value of this parameter is determined by the caller.</param>
        /// <returns>True if updating the property value succeeds; else false.</returns>
        public object Execute(object sender, object parameter)
        {
            object targetObject;
            if (this.GetValue(ChangePerspexPropertyAction.TargetObjectProperty) != PerspexProperty.UnsetValue)
            {
                targetObject = this.TargetObject;
            }
            else
            {
                targetObject = sender as PerspexObject;
            }

            if (targetObject == null || this.PropertyName == null)
            {
                return false;
            }

            this.UpdatePerspexPropertyValue(targetObject);
            return true;
        }

        private void UpdatePerspexPropertyValue(object targetObject)
        {
            this.ValidatePerspexProperty(this.PropertyName);

            Exception innerException = null;
            try
            {
                object result = null;
                string valueAsString = null;
                Type propertyType = this.PropertyName.PropertyType;
                TypeInfo propertyTypeInfo = propertyType.GetTypeInfo();
                if (this.Value == null)
                {
                    // The result can be null if the type is generic (nullable), or the default value of the type in question
                    result = propertyTypeInfo.IsValueType ? Activator.CreateInstance(propertyType) : null;
                }
                else if (propertyTypeInfo.IsAssignableFrom(this.Value.GetType().GetTypeInfo()))
                {
                    result = this.Value;
                }
                else
                {
                    valueAsString = this.Value.ToString();
                    result = propertyTypeInfo.IsEnum ? Enum.Parse(propertyType, valueAsString, false) :
                        TypeConverterHelper.Convert(valueAsString, propertyType.FullName);
                }

                var pespexObject = targetObject as PerspexObject;
                if (pespexObject != null)
                {
                    pespexObject.SetValue(this.PropertyName, result);
                }
            }
            catch (FormatException e)
            {
                innerException = e;
            }
            catch (ArgumentException e)
            {
                innerException = e;
            }

            if (innerException != null)
            {
                throw new ArgumentException(string.Format(
                    CultureInfo.CurrentCulture,
                    "Cannot assign value of type {0} to property {1} of type {2}. The {1} property can be assigned only values of type {2}.",
                    this.Value?.GetType().Name ?? "null",
                    this.PropertyName,
                    targetObject?.GetType().Name ?? "null"),
                    innerException);
            }
        }

        /// <summary>
        /// Ensures the property is not null and can be written to.
        /// </summary>
        private void ValidatePerspexProperty(PerspexProperty property)
        {
            if (property == null)
            {
                throw new ArgumentException(string.Format(
                    CultureInfo.CurrentCulture,
                    "Cannot find a property named {0}.",
                    this.PropertyName));
            }
            else if (property.IsReadOnly)
            {
                throw new ArgumentException(string.Format(
                    CultureInfo.CurrentCulture,
                    "Cannot find a property named {0}.",
                    this.PropertyName));
            }
        }
    }
}