﻿/*
 *                     GNU AFFERO GENERAL PUBLIC LICENSE
 *                       Version 3, 19 November 2007
 *  Copyright (C) 2007 Free Software Foundation, Inc. <https://fsf.org/>
 *  Everyone is permitted to copy and distribute verbatim copies
 *  of this license document, but changing it is not allowed.
 */
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace UVtools.Core
{
    #region LayerIssue Class

    public class IssuesDetectionConfiguration
    {
        public IslandDetectionConfiguration IslandConfig { get; }
        public OverhangDetectionConfiguration OverhangConfig { get; }
        public ResinTrapDetectionConfiguration ResinTrapConfig { get; }
        public TouchingBoundDetectionConfiguration TouchingBoundConfig { get; }
        public bool EmptyLayerConfig { get; }

        public IssuesDetectionConfiguration(IslandDetectionConfiguration islandConfig, OverhangDetectionConfiguration overhangConfig, ResinTrapDetectionConfiguration resinTrapConfig, TouchingBoundDetectionConfiguration touchingBoundConfig, bool emptyLayerConfig)
        {
            IslandConfig = islandConfig;
            OverhangConfig = overhangConfig;
            ResinTrapConfig = resinTrapConfig;
            TouchingBoundConfig = touchingBoundConfig;
            EmptyLayerConfig = emptyLayerConfig;
        }
    }

    public class IslandDetectionConfiguration
    {
        /// <summary>
        /// Gets or sets if the detection is enabled
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Gets or sets a list of layers to check for islands, absent layers will not be checked.
        /// Set to null to check every layer
        /// </summary>
        public List<uint> WhiteListLayers { get; set; } = null;

        /// <summary>
        /// Gets or sets the binary threshold, all pixels below this value will turn in black, otherwise white
        /// Set to 0 to disable this operation 
        /// </summary>
        public byte BinaryThreshold { get; set; } = 1;

        /// <summary>
        /// Gets the required area size (x*y) to consider process a island (0-255)
        /// </summary>
        public byte RequiredAreaToProcessCheck { get; set; } = 1;

        /// <summary>
        /// Gets the required brightness for check a pixel under a island (0-255)
        /// </summary>
        public byte RequiredPixelBrightnessToProcessCheck { get; set; } = 10;

        /// <summary>
        /// Gets the required number of pixels to support a island and discard it as a issue (0-255)
        /// </summary>
        public byte RequiredPixelsToSupport { get; set; } = 10;

        /// <summary>
        /// Gets the required multiplier from the island pixels to support same island and discard it as a issue
        /// </summary>
        public decimal RequiredPixelsToSupportMultiplier { get; set; } = 0.25m;

        /// <summary>
        /// Gets the required brightness of supporting pixels to count as a valid support (0-255)
        /// </summary>
        public byte RequiredPixelBrightnessToSupport { get; set; } = 150;

        /// <summary>
        /// Gets the setting for whether or not diagonal bonds are considered when evaluation islands.
        /// If true, all 8 neighbors of a pixel (including diagonals) will be considered when finding
        /// individual components on the layer, if false only 4 neighbors (right, left, above, below)
        /// will be considered..
        /// </summary>
        public bool AllowDiagonalBonds  { get; set; } = false;

        public IslandDetectionConfiguration(bool enabled = true)
        {
            Enabled = enabled;
        }
    }

    /// <summary>
    /// Overhang configuration
    /// </summary>
    public class OverhangDetectionConfiguration
    {
        /// <summary>
        /// Gets or sets if the detection is enabled
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Gets or sets a list of layers to check for overhangs, absent layers will not be checked.
        /// Set to null to check every layer
        /// </summary>
        public List<uint> WhiteListLayers { get; set; } = null;

        /// <summary>
        /// Gets or sets if should take in consideration the islands, if yes a island can't be a overhang at same time, otherwise islands and overhangs can be shared
        /// </summary>
        public bool IndependentFromIslands { get; set; } = true;

        /// <summary>
        /// After compute overhangs, masses with a number of pixels bellow this number will be discarded (Not a overhang)
        /// </summary>
        public byte RequiredPixelsToConsider { get; set; } = 1;
        
        /// <summary>
        /// Previous layer will be subtracted from current layer, after will erode by this value.
        /// The survived pixels are potential overhangs.
        /// </summary>
        public byte ErodeIterations { get; set; } = 40;

        public OverhangDetectionConfiguration(bool enabled = true)
        {
            Enabled = enabled;
        }
    }

    public class ResinTrapDetectionConfiguration
    {
        /// <summary>
        /// Gets or sets if the detection is enabled
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Gets or sets the binary threshold, all pixels below this value will turn in black, otherwise white
        /// Set to 0 to disable this operation
        /// </summary>
        public byte BinaryThreshold { get; set; } = 127;

        /// <summary>
        /// Gets the required area size (x*y) to consider process a hollow area (0-255)
        /// </summary>
        public byte RequiredAreaToProcessCheck { get; set; } = 1;

        /// <summary>
        /// Gets the number of black pixels required to consider a drain
        /// </summary>
        public byte RequiredBlackPixelsToDrain { get; set; } = 10;

        /// <summary>
        /// Gets the maximum pixel brightness to be a drain pixel (0-150)
        /// </summary>
        public byte MaximumPixelBrightnessToDrain { get; set; } = 30;

        public ResinTrapDetectionConfiguration(bool enabled = true)
        {
            Enabled = enabled;
        }
    }


    public class TouchingBoundDetectionConfiguration
    {
        /// <summary>
        /// Gets if the detection is enabled
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Gets the maximum pixel brightness to be a touching bound
        /// </summary>
        public byte MaximumPixelBrightness { get; set; } = 200;

        public TouchingBoundDetectionConfiguration(bool enabled = true)
        {
            Enabled = enabled;
        }
    }


    public class LayerIssue : IEnumerable<Point>
    {
        public enum IssueType : byte
        {
            Island,
            Overhang,
            ResinTrap,
            TouchingBound,
            EmptyLayer,
            //HoleSandwich,
        }

        /// <summary>
        /// Gets the parent layer
        /// </summary>
        public Layer Layer { get; }

        /// <summary>
        /// Gets the layer index
        /// </summary>
        public uint LayerIndex => Layer.Index;

        /// <summary>
        /// Gets the issue type associated
        /// </summary>
        public IssueType Type { get; }

        /// <summary>
        /// Gets the pixels points containing the issue
        /// </summary>
        public Point[] Pixels { get; }

        public int PixelsCount => Pixels?.Length ?? 0;

        /// <summary>
        /// Gets the bounding rectangle of the pixel area
        /// </summary>
        public Rectangle BoundingRectangle { get; }

        /// <summary>
        /// Gets the X coordinate for the first point, -1 if doesn't exists
        /// </summary>
        public int X => HaveValidPoint ? Pixels[0].X : -1;

        /// <summary>
        /// Gets the Y coordinate for the first point, -1 if doesn't exists
        /// </summary>
        public int Y => HaveValidPoint ? Pixels[0].Y : -1;

        /// <summary>
        /// Gets the XY point for first point
        /// </summary>
        public Point FirstPoint => HaveValidPoint ? Pixels[0] : new Point(-1, -1);
        public string FirstPointStr => $"{FirstPoint.X}, {FirstPoint.Y}";

        /// <summary>
        /// Gets the number of pixels on this issue
        /// </summary>
        public uint Size
        {
            get
            {
                if (Type == IssueType.ResinTrap && !BoundingRectangle.IsEmpty)
                {
                    return (uint)(BoundingRectangle.Width * BoundingRectangle.Height);
                }

                if (ReferenceEquals(Pixels, null)) return 0;
                return (uint)Pixels.Length;
            }
        }

        /// <summary>
        /// Check if this issue have a valid start point to show
        /// </summary>
        public bool HaveValidPoint => !ReferenceEquals(Pixels, null) && Pixels.Length > 0;

        public LayerIssue(Layer layer, IssueType type, Point[] pixels = null, Rectangle boundingRectangle = new Rectangle())
        {
            Layer = layer;
            Type = type;
            Pixels = pixels;
            BoundingRectangle = boundingRectangle;
        }

        public Point this[uint index] => Pixels[index];

        public Point this[int index] => Pixels[index];

        public IEnumerator<Point> GetEnumerator()
        {
            return ((IEnumerable<Point>)Pixels).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override string ToString()
        {
            return $"{nameof(Type)}: {Type}, Layer: {Layer.Index}, {nameof(X)}: {X}, {nameof(Y)}: {Y}, {nameof(Size)}: {Size}";
        }
    }
    #endregion

    #region LayerHollowArea

    public class LayerHollowArea : IEnumerable<Point>
    {
        public enum AreaType : byte
        {
            Unknown = 0,
            Trap,
            Drain
        }
        /// <summary>
        /// Gets area pixels
        /// </summary>
        public Point[] Contour { get; }

        public Rectangle BoundingRectangle { get; }

        public AreaType Type { get; set; } = AreaType.Unknown;

        public bool Processed { get; set; }

        #region Indexers
        public Point this[uint index]
        {
            get => index < Contour.Length ? Contour[index] : Point.Empty;
            set => Contour[index] = value;
        }

        public Point this[int index]
        {
            get => index < Contour.Length ? Contour[index] : Point.Empty;
            set => Contour[index] = value;
        }

        public Point this[uint x, uint y]
        {
            get
            {
                for (uint i = 0; i < Contour.Length; i++)
                {
                    if (Contour[i].X == x && Contour[i].Y == y) return Contour[i];
                }
                return Point.Empty;
            }
        }

        public Point this[int x, int y] => this[(uint)x, (uint)y];

        public Point this[Point point] => this[point.X, point.Y];

        #endregion

        public IEnumerator<Point> GetEnumerator()
        {
            return ((IEnumerable<Point>)Contour).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public LayerHollowArea()
        {
        }

        public LayerHollowArea(Point[] contour, Rectangle boundingRectangle, AreaType type = AreaType.Unknown)
        {
            Contour = contour;
            BoundingRectangle = boundingRectangle;
            Type = type;
        }
    }
    #endregion
}
