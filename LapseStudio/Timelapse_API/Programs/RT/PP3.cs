﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;

namespace Timelapse_API
{
    /// <summary>
    /// Stores and handles all data from a PP3 file
    /// </summary>
    [Serializable()]
	public partial class PP3
    {
        /// <summary>
        /// Path to the orignal file
        /// </summary>
        public string Path
        {
            get { return _Path; }
            internal set { _Path = value; }
        }
        /// <summary>
        /// The file version of this PP3
        /// </summary>
        public int FileVersion
        {
            get { return _FileVersion; }
            internal set { _FileVersion = value; }
        }
        /// <summary>
        /// The compensation value of this PP3
        /// </summary>
        public double Compensation
        {
            get { return _Compensation; }
            internal set { _Compensation = value; }
        }
        /// <summary>
        /// The newly calculated compensation value of this PP3
        /// </summary>
        public double NewCompensation
        {
            get { return _NewCompensation; }
            internal set { _NewCompensation = value; }
        }
        /// <summary>
        /// List of all PP3 values
        /// </summary>
        public Dictionary<string, PP3entry> Values
        {
            get { return _Values; }
            internal set { _Values = value; }
        }

        private string _Path;
        private int _FileVersion;
        private double _Compensation;
        private double _NewCompensation;
        private Dictionary<string, PP3entry> _Values;
        
        /// <summary>
        /// Initiates a new and empty PP3 file
        /// </summary>
        public PP3()
        {
            Values = new Dictionary<string, PP3entry>();
        }

        /// <summary>
        /// Initiates and reads a new PP3 file
        /// </summary>
        /// <param name="Path">Path to the PP3 file</param>
        public PP3(string Path)
        {
            Values = new Dictionary<string, PP3entry>();
            this.Path = Path;
            Read();
        }

        /// <summary>
        /// Write all values to disk
        /// </summary>
        /// <param name="Path">Path to the file</param>
        public void Write(string Path)
        {
            using (StreamWriter Writer = new StreamWriter(Path))
            {
                switch (FileVersion)
                {
                    case 302:
                        Write302(Writer);
                        break;
                    case 308:
                        Write308(Writer);
                        break;
                    case 309:
                        Write309(Writer);
                        break;
                    case 310:
                        Write310(Writer);
                        break;
                    case 315:
                    case 316:
                        Write316(Writer);
                        break;
                    case 317:
                    case 318:
                        Write318(Writer);
                        break;
                    case 319:
                        Write319(Writer);
                        break;
                    case 321:
                        Write321(Writer);
                        break;
                    case 326:
                        Write326(Writer);
                        break;

                    default:
                        throw new NotSupportedException("This fileversion is not supported!");
                }
            }
        }

        /// <summary>
        /// Copies all values
        /// </summary>
        /// <returns>a new PP3 with values duplicated from this PP3</returns>
        public PP3 Copy()
        {
            PP3 output = new PP3();
            output.Path = this.Path;
            output.FileVersion = this.FileVersion;
            output.Compensation = this.Compensation;
            output.NewCompensation = this.NewCompensation;
            output.Values = new Dictionary<string, PP3entry>(this.Values);
            return output;
        }
        
        private void Add(PP3entry val)
        {
            Values.Add(val.Name, val);
        }

        private void Write(StreamWriter writer, string header, string name)
        {
            CultureInfo culture = new CultureInfo("en-US");
            string digits = "0.################"; // up to 16 digits

            PP3entry entry = Values[header + "." + name];
            object val = entry.Value;
            string valstring;
            if (val is bool)
            {
                valstring = val.ToString().ToLower();

            }
            else if (val is double)
            {
                valstring = ((double)val).ToString(digits, culture);
            }
            else if (val is int)
            {
                valstring = val.ToString();
            }
            else if (val is string)
            {
                valstring = val.ToString();
            }
            else if (val is PP3Curve)
            {
                valstring = ((PP3Curve)val).ToString();
            }
            else
            {
                throw new InvalidOperationException("Value " + header + "." + name + " has unknown type: " + val);
            }
            writer.WriteLine(name + "=" + valstring);
        }

        private string GetValue(string line)
        {
            return line.Substring(line.LastIndexOf("=") + 1);
        }

        private void Read()
        {
            string[] lines = File.ReadAllLines(Path);

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].ToLower().StartsWith("[version]"))
                {
                    for (i++; i < lines.Length; i++)
                    {
                        if (lines[i].ToLower().StartsWith("version")) { FileVersion = Convert.ToInt32(lines[i].Substring(lines[i].LastIndexOf("=") + 1)); goto VersionCheck; }
                    }
                }
            }

        VersionCheck:
            if (FileVersion == 0) { throw new FileLoadException("Couldn't read fileversion!"); }

            switch (FileVersion)
            {
                case 302:
                    Read302(lines);
                    break;
                case 308:
                    Read308(lines);
                    break;
                case 309:
                    Read309(lines);
                    break;
                case 310:
                    Read310(lines);
                    break;
                case 315:
                case 316:
                    Read316(lines);
                    break;
                case 317:
                case 318:
                    Read318(lines);
                    break;
                case 319:
                    Read319(lines);
                    break;
                case 321:
                    Read321(lines);
                    break;
                case 326:
                    Read326(lines);
                    break;

                default:
                    throw new NotSupportedException("This fileversion is not supported!");
            }
        }

        /// <summary>
        /// Stores data for a PP3 value
        /// </summary>
        [Serializable()]
        public class PP3entry
        {
            public string Name
            {
                get { return _Name; }
                private set { _Name = value; }
            }
            public object Value;
            public object min
            {
                get { return _min; }
                private set { _min = value; }
            }
            public object max
            {
                get { return _max; }
                private set { _max = value; }
            }

            private string _Name;
            private object _min;
            private object _max;

            public PP3entry(string Name, object Value, object min, object max)
            {
                this.Name = Name;
                this.Value = Value;
                this.min = min;
                this.max = max;
            }
        }
    }
}
