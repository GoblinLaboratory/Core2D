﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Core2D.Renderer;
using Core2D.Style;

namespace Core2D
{
    /// <summary>
    /// Defines drawable shape contract.
    /// </summary>
    public interface IDrawable
    {
        /// <summary>
        /// Get or sets shape drawing style.
        /// </summary>
        IShapeStyle Style { get; set; }

        /// <summary>
        /// Gets or sets flag indicating whether shape is stroked.
        /// </summary>
        bool IsStroked { get; set; }

        /// <summary>
        /// Gets or sets flag indicating whether shape is filled.
        /// </summary>
        bool IsFilled { get; set; }

        /// <summary>
        /// Get or sets shape matrix transform.
        /// </summary>
        IMatrixObject Transform { get; set; }

        /// <summary>
        /// Begins matrix transform.
        /// </summary>
        /// <param name="dc">The generic drawing context object.</param>
        /// <param name="renderer">The generic renderer object used to draw shape.</param>
        /// <returns>The previous transform state.</returns>
        object BeginTransform(object dc, IShapeRenderer renderer);

        /// <summary>
        /// Ends matrix transform.
        /// </summary>
        /// <param name="dc">The generic drawing context object.</param>
        /// <param name="renderer">The generic renderer object used to draw shape.</param>
        /// <param name="state">The previous transform state.</param>
        void EndTransform(object dc, IShapeRenderer renderer, object state);

        /// <summary>
        /// Draws shape using current <see cref="IShapeRenderer"/>.
        /// </summary>
        /// <param name="dc">The generic drawing context object.</param>
        /// <param name="renderer">The generic renderer object used to draw shape.</param>
        /// <param name="dx">The X axis draw position offset.</param>
        /// <param name="dy">The Y axis draw position offset.</param>
        /// <param name="db">The properties database.</param>
        /// <param name="r">The database record.</param>
        void Draw(object dc, IShapeRenderer renderer, double dx, double dy, object db, object r);

        /// <summary>
        /// Invalidates shape renderer cache.
        /// </summary>
        /// <param name="renderer">The generic renderer object used to draw shape.</param>
        /// <param name="dx">The X axis draw position offset.</param>
        /// <param name="dy">The Y axis draw position offset.</param>
        /// <returns>Returns true if shape was invalidated; otherwise, returns false.</returns>
        bool Invalidate(IShapeRenderer renderer, double dx, double dy);
    }
}
