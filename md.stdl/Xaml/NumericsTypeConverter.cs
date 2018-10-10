using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace md.stdl.Xaml
{
    /// <inheritdoc />
    /// <summary>
    /// String converter for <see cref="Vector2" />
    /// </summary>
    public class Vector2Converter : TypeConverter
    {

        /// <inheritdoc />
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(string))
                return true;
            return base.CanConvertTo(context, destinationType);
        }

        /// <inheritdoc />
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is Vector2 v)
            {
                return $"{v.X.ToString(CultureInfo.InvariantCulture)}, {v.Y.ToString(CultureInfo.InvariantCulture)}";
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        /// <inheritdoc />
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string)) return true;
            return base.CanConvertFrom(context, sourceType);
        }

        /// <inheritdoc />
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string sval)
            {
                var comps = sval.Split(new[] {", "}, StringSplitOptions.RemoveEmptyEntries);
                if (comps.Length < 2) return Vector2.Zero;
                return new Vector2(
                    float.Parse(comps[0].Trim(), CultureInfo.InvariantCulture),
                    float.Parse(comps[1].Trim(), CultureInfo.InvariantCulture)
                );
            }
            return base.ConvertFrom(context, culture, value);
        }
    }

    /// <inheritdoc />
    /// <summary>
    /// String converter for <see cref="Vector3" />
    /// </summary>
    public class Vector3Converter : TypeConverter
    {
        /// <inheritdoc />
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(string))
                return true;
            return base.CanConvertTo(context, destinationType);
        }

        /// <inheritdoc />
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is Vector3 v)
            {
                return $"{v.X.ToString(CultureInfo.InvariantCulture)}, {v.Y.ToString(CultureInfo.InvariantCulture)}, {v.Z.ToString(CultureInfo.InvariantCulture)}";
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        /// <inheritdoc />
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string)) return true;
            return base.CanConvertFrom(context, sourceType);
        }

        /// <inheritdoc />
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string sval)
            {
                var comps = sval.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                if (comps.Length < 3) return Vector3.Zero;
                return new Vector3(
                    float.Parse(comps[0].Trim(), CultureInfo.InvariantCulture),
                    float.Parse(comps[1].Trim(), CultureInfo.InvariantCulture),
                    float.Parse(comps[2].Trim(), CultureInfo.InvariantCulture)
                );
            }
            return base.ConvertFrom(context, culture, value);
        }
    }

    /// <inheritdoc />
    /// <summary>
    /// String converter for <see cref="Vector4" />
    /// </summary>
    public class Vector4Converter : TypeConverter
    {
        /// <inheritdoc />
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(string))
                return true;
            return base.CanConvertTo(context, destinationType);
        }

        /// <inheritdoc />
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is Vector4 v)
            {
                return $"{v.X.ToString(CultureInfo.InvariantCulture)}, {v.Y.ToString(CultureInfo.InvariantCulture)}, {v.Z.ToString(CultureInfo.InvariantCulture)}, {v.W.ToString(CultureInfo.InvariantCulture)}";
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        /// <inheritdoc />
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string)) return true;
            return base.CanConvertFrom(context, sourceType);
        }

        /// <inheritdoc />
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string sval)
            {
                var comps = sval.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                if (comps.Length < 4) return Vector4.Zero;
                return new Vector4(
                    float.Parse(comps[0].Trim(), CultureInfo.InvariantCulture),
                    float.Parse(comps[1].Trim(), CultureInfo.InvariantCulture),
                    float.Parse(comps[2].Trim(), CultureInfo.InvariantCulture),
                    float.Parse(comps[3].Trim(), CultureInfo.InvariantCulture)
                );
            }
            return base.ConvertFrom(context, culture, value);
        }
    }

    /// <inheritdoc />
    /// <summary>
    /// String converter for <see cref="Quaternion" />
    /// </summary>
    public class QuaternionConverter : TypeConverter
    {
        /// <inheritdoc />
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(string))
                return true;
            return base.CanConvertTo(context, destinationType);
        }

        /// <inheritdoc />
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is Quaternion v)
            {
                return $"{v.X.ToString(CultureInfo.InvariantCulture)}, {v.Y.ToString(CultureInfo.InvariantCulture)}, {v.Z.ToString(CultureInfo.InvariantCulture)}, {v.W.ToString(CultureInfo.InvariantCulture)}";
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        /// <inheritdoc />
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string)) return true;
            return base.CanConvertFrom(context, sourceType);
        }

        /// <inheritdoc />
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string sval)
            {
                var comps = sval.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                if (comps.Length < 4) return Quaternion.Identity;
                return new Quaternion(
                    float.Parse(comps[0].Trim(), CultureInfo.InvariantCulture),
                    float.Parse(comps[1].Trim(), CultureInfo.InvariantCulture),
                    float.Parse(comps[2].Trim(), CultureInfo.InvariantCulture),
                    float.Parse(comps[3].Trim(), CultureInfo.InvariantCulture)
                );
            }
            return base.ConvertFrom(context, culture, value);
        }
    }

    /// <inheritdoc />
    /// <summary>
    /// String converter for <see cref="Matrix4x4" />
    /// </summary>
    public class Matrix4x4Converter : TypeConverter
    {
        /// <inheritdoc />
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(string))
                return true;
            return base.CanConvertTo(context, destinationType);
        }

        /// <inheritdoc />
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is Matrix4x4 m)
            {
                var res = "";
                res += m.M11.ToString(CultureInfo.InvariantCulture) + ", ";
                res += m.M12.ToString(CultureInfo.InvariantCulture) + ", ";
                res += m.M13.ToString(CultureInfo.InvariantCulture) + ", ";
                res += m.M14.ToString(CultureInfo.InvariantCulture) + ", ";
                res += m.M21.ToString(CultureInfo.InvariantCulture) + ", ";
                res += m.M22.ToString(CultureInfo.InvariantCulture) + ", ";
                res += m.M23.ToString(CultureInfo.InvariantCulture) + ", ";
                res += m.M24.ToString(CultureInfo.InvariantCulture) + ", ";
                res += m.M31.ToString(CultureInfo.InvariantCulture) + ", ";
                res += m.M32.ToString(CultureInfo.InvariantCulture) + ", ";
                res += m.M33.ToString(CultureInfo.InvariantCulture) + ", ";
                res += m.M34.ToString(CultureInfo.InvariantCulture) + ", ";
                res += m.M41.ToString(CultureInfo.InvariantCulture) + ", ";
                res += m.M42.ToString(CultureInfo.InvariantCulture) + ", ";
                res += m.M43.ToString(CultureInfo.InvariantCulture) + ", ";
                res += m.M44.ToString(CultureInfo.InvariantCulture);
                return res;
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        /// <inheritdoc />
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string)) return true;
            return base.CanConvertFrom(context, sourceType);
        }

        /// <inheritdoc />
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string sval)
            {
                var comps = sval.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                if (comps.Length < 16) return Matrix4x4.Identity;
                return new Matrix4x4
                {
                    M11 = float.Parse(comps[0].Trim(), CultureInfo.InvariantCulture),
                    M12 = float.Parse(comps[1].Trim(), CultureInfo.InvariantCulture),
                    M13 = float.Parse(comps[2].Trim(), CultureInfo.InvariantCulture),
                    M14 = float.Parse(comps[3].Trim(), CultureInfo.InvariantCulture),
                    M21 = float.Parse(comps[4].Trim(), CultureInfo.InvariantCulture),
                    M22 = float.Parse(comps[5].Trim(), CultureInfo.InvariantCulture),
                    M23 = float.Parse(comps[6].Trim(), CultureInfo.InvariantCulture),
                    M24 = float.Parse(comps[7].Trim(), CultureInfo.InvariantCulture),
                    M31 = float.Parse(comps[8].Trim(), CultureInfo.InvariantCulture),
                    M32 = float.Parse(comps[9].Trim(), CultureInfo.InvariantCulture),
                    M33 = float.Parse(comps[10].Trim(), CultureInfo.InvariantCulture),
                    M34 = float.Parse(comps[11].Trim(), CultureInfo.InvariantCulture),
                    M41 = float.Parse(comps[12].Trim(), CultureInfo.InvariantCulture),
                    M42 = float.Parse(comps[13].Trim(), CultureInfo.InvariantCulture),
                    M43 = float.Parse(comps[14].Trim(), CultureInfo.InvariantCulture),
                    M44 = float.Parse(comps[15].Trim(), CultureInfo.InvariantCulture),
                };
            }
            return base.ConvertFrom(context, culture, value);
        }
    }
}
