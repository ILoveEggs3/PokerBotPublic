using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BotImageHelper
{
    public class ConstantFileGenerator
    {
        const string GENERATED_COMMENT_FORMAT =
            "/************************************\r\n" +
            "{0}\r\n" +
            "************************************/\r\n";
        const string NAMESPACE_DEFINITION_FORMAT =
            "namespace {0}\r\n" +
            "{{\r\n" +
            "\r\n" +
            "{1}\r\n" +
            "}}\r\n";
        const string CLASS_DEFINITION_FORMAT =
            "public class {0}\r\n" +
            "{{\r\n" +
            "{1}\r\n" +
            "}}";
        const string CLASS_CONSTRUCTOR_FORMAT =
            "public class {0}()\r\n" +
            "{{\r\n" +
            "{1}\r\n" +
            "}}";
        public enum RegionType
        {
            FourRows,
            AbstractFourRows,
            OverrrideRectangle,
            AbstractRectangle,
            StandAloneRectangle
        }

        public static readonly Dictionary<RegionType, AbstractRegionType> PRegionTypes = new Dictionary<RegionType, AbstractRegionType>()
        {
            { RegionType.FourRows, new FourRowsRegionType() },
            { RegionType.OverrrideRectangle, new OverrideRectangleRegionType() },
            { RegionType.AbstractRectangle, new AbstractRectangleRegionType() },
            { RegionType.StandAloneRectangle, new StandAloneRectangleRegionType() }
        };
        public abstract class AbstractRegionType
        {
            const string GENERATED_REGION_FORMAT =
                "#region {0}\r\n\r\n" +
                "{1}" +
                "\r\n#endregion\r\n";
            public string PName;
            public Rectangle PValues;
            protected AbstractRegionType(string _name, Rectangle _rec)
            {
                PName = _name;
                PValues = _rec;
            }
            public static AbstractRegionType NewRegion(RegionType _type, string _name, Rectangle _rec)
            {
                switch (_type)
                {
                    case RegionType.FourRows:
                        return new FourRowsRegionType(_name, _rec);
                    case RegionType.AbstractFourRows:
                        throw new NotImplementedException();
                    case RegionType.OverrrideRectangle:
                        return new OverrideRectangleRegionType(_name, _rec);
                    case RegionType.AbstractRectangle:
                        return new AbstractRectangleRegionType(_name, _rec);
                    case RegionType.StandAloneRectangle:
                        return new StandAloneRectangleRegionType(_name, _rec);
                    default:
                        throw new Exception("Unexpected value received");
                }
            }
            public override string ToString()
            {
                PName = PName.ToUpper();
                var returnStr = GetString();
                returnStr = String.Format(GENERATED_REGION_FORMAT, PName, returnStr);
                return returnStr;
            }
            protected abstract string GetString();
            public abstract AbstractRegionType parse(string _str);
            public abstract bool IsMatch(string _str);
            public static AbstractRegionType Parse(string _str)
            {
                var it = PRegionTypes.GetEnumerator();
                bool found = false;
                while (it.MoveNext() && !(found = it.Current.Value.IsMatch(_str))) ;
                if (!found)
                    throw new Exception("Unrecognised region format type");

                return NewRegion(it.Current.Key, "qwe", Rectangle.Empty).parse(_str);
            }
        }
        private class FourRowsRegionType : AbstractRegionType
        {
            public FourRowsRegionType(AbstractRegionType _region) : base(_region.PName, _region.PValues) { }
            public FourRowsRegionType(string _name = "qwe", Rectangle _rec = new Rectangle()) : base(_name, _rec) { }
            public override bool IsMatch(string _str)
            {
                Regex reg = new Regex(@"_X = [0-9]+;\r?\n");
                return reg.IsMatch(_str);
            }

            public override AbstractRegionType parse(string _str)
            {
                int X = Int32.Parse(Regex.Match(Regex.Match(_str, "_X = [0-9]+;\r?\n").Value, "[0-9]+").Value);
                int Y = Int32.Parse(Regex.Match(Regex.Match(_str, "_Y = [0-9]+;\r?\n").Value, "[0-9]+").Value);
                int Width = Int32.Parse(Regex.Match(Regex.Match(_str, "_WIDTH = [0-9]+;\r?\n").Value, "[0-9]+").Value);
                int Height = Int32.Parse(Regex.Match(Regex.Match(_str, "_HEIGHT = [0-9]+;\r?\n").Value, "[0-9]+").Value);

                string regionName = Regex.Match(_str, "[A-Z].*_X").Value;
                regionName = regionName.Substring(0, regionName.Length - 2);

                return new FourRowsRegionType(regionName, new Rectangle(X, Y, Width, Height));
            }

            protected override string GetString()
            {
                const string GENERATED_ATTRIBUTE_FORMAT =
                    "public int {0}_X = {1};\r\n" +
                    "public int {0}_Y = {2};\r\n" +
                    "public int {0}_WIDTH = {3};\r\n" +
                    "public int {0}_HEIGHT = {4};\r\n";
                PName = PName.ToUpper();
                var returnStr = String.Format(GENERATED_ATTRIBUTE_FORMAT, PName, PValues.X.ToString(), PValues.Y.ToString(), PValues.Width.ToString(), PValues.Height.ToString());
                return returnStr;
            }
        }
        private class OverrideRectangleRegionType : AbstractRegionType
        {
            public OverrideRectangleRegionType(AbstractRegionType _region) : base(_region.PName, _region.PValues) { }
            public OverrideRectangleRegionType(string _name = "qwe", Rectangle _rec = new Rectangle()) : base(_name, _rec) { }
            public override bool IsMatch(string _str)
            {
                Regex reg = new Regex(@".*private static readonly Rectangle FF(.*) = new Rectangle\((.*), ?(.*), ?(.*), ?(.*) ?\);.*");
                return reg.IsMatch(_str);
            }

            public override AbstractRegionType parse(string _str)
            {
                List<int> values = new List<int>();
                Regex nameReg = new Regex(" FF[A-Z,0-9,_]+");//(" FF[A-Z,0-9]+ ");
                Regex rectangleValuesReg = new Regex(@"Rectangle\([0-9]+, ?[0-9]+, ?[0-9]+, ?[0-9]+ ?\)");
                Regex valuesReg = new Regex(@"[0-9]+");
                var name = nameReg.Match(_str).Value;
                name = name.Substring(3, name.Length - 3);
                var matches = valuesReg.Matches(rectangleValuesReg.Match(_str).Value);
                foreach (Match m in matches)
                {
                    values.Add(Int32.Parse(m.Value));
                }
                if (values.Count != 4)
                {
                    throw new Exception("Unexpected value count");
                }
                return new OverrideRectangleRegionType(name, new Rectangle(values[0], values[1], values[2], values[3]));
            }

            protected override string GetString()
            {
                const string GENERATED_ATTRIBUTE_FORMAT =
                        "private static readonly Rectangle FF{0} = new Rectangle({1}, {2}, {3}, {4});\r\n" +
                        "public override Rectangle {0} {{ get {{ return FF{0}; }} }}\r\n";
                PName = PName.ToUpper();
                var returnStr = String.Format(GENERATED_ATTRIBUTE_FORMAT, PName, PValues.X.ToString(), PValues.Y.ToString(), PValues.Width.ToString(), PValues.Height.ToString());
                return returnStr;
            }
        }
        private class AbstractRectangleRegionType : AbstractRegionType
        {
            //public abstract Rectangle HAND_CARD1_VALUE { get; }
            public AbstractRectangleRegionType(AbstractRegionType _region) : base(_region.PName, _region.PValues) { }
            public AbstractRectangleRegionType(string _name = "qwe", Rectangle _rec = new Rectangle()) : base(_name, _rec) { }
            public override bool IsMatch(string _str)
            {
                Regex reg = new Regex(@".*public abstract Rectangle .*? { get; }");
                return reg.IsMatch(_str);
            }

            public override AbstractRegionType parse(string _str)
            {
                Regex nameReg = new Regex("Rectangle [A-Z,0-9,_]+");//(" FF[A-Z,0-9]+ ");
                var name = nameReg.Match(_str).Value;
                name = name.Substring("Rectangle ".Length, name.Length - "Rectangle ".Length);
                return new OverrideRectangleRegionType(name, new Rectangle());
            }

            protected override string GetString()
            {
                const string GENERATED_ATTRIBUTE_FORMAT =
                        "public abstract Rectangle {0} {{ get; }}\r\n";
                PName = PName.ToUpper();
                var returnStr = String.Format(GENERATED_ATTRIBUTE_FORMAT, PName);
                return returnStr;
            }
        }
        private class StandAloneRectangleRegionType : AbstractRegionType
        {
            public StandAloneRectangleRegionType(AbstractRegionType _region) : base(_region.PName, _region.PValues) { }
            public StandAloneRectangleRegionType(string _name = "qwe", Rectangle _rec = new Rectangle()) : base(_name, _rec) { }
            //public static readonly Rectangle PPLAYER_1_BET = new Rectangle(440, 261, 50, 6);
            public override bool IsMatch(string _str)
            {
                Regex reg = new Regex(@".*public static readonly Rectangle P(.*) = new Rectangle\((.*), ?(.*), ?(.*), ?(.*) ?\);.*");
                return reg.IsMatch(_str);
            }

            public override AbstractRegionType parse(string _str)
            {
                List<int> values = new List<int>();
                Regex nameReg = new Regex(" P[A-Z,0-9,_]+");
                Regex rectangleValuesReg = new Regex(@"Rectangle\([0-9]+, ?[0-9]+, ?[0-9]+, ?[0-9]+ ?\)");
                Regex valuesReg = new Regex(@"[0-9]+");
                var name = nameReg.Match(_str).Value;
                name = name.Substring(2, name.Length - 2);
                var matches = valuesReg.Matches(rectangleValuesReg.Match(_str).Value);
                foreach (Match m in matches)
                {
                    values.Add(Int32.Parse(m.Value));
                }
                if (values.Count != 4)
                {
                    throw new Exception("Unexpected value count");
                }
                return new OverrideRectangleRegionType(name, new Rectangle(values[0], values[1], values[2], values[3]));
            }

            protected override string GetString()
            {
                const string GENERATED_ATTRIBUTE_FORMAT =
                        "public static readonly Rectangle P{0} = new Rectangle({1}, {2}, {3}, {4});\r\n";
                PName = PName.ToUpper();
                var returnStr = String.Format(GENERATED_ATTRIBUTE_FORMAT, PName, PValues.X.ToString(), PValues.Y.ToString(), PValues.Width.ToString(), PValues.Height.ToString());
                return returnStr;
            }
        }

        static readonly string HEADER_GENERATED_COMMENT =
            String.Format(GENERATED_COMMENT_FORMAT, "*****GENERATED BY BOTIMAGEHELPER*****");

        static readonly Regex classBodyRegex = new Regex("(.*class.*\n.*{)((.*\n)*)(.*}(.*\n)*.*})");

        private string finalString = "";
        private string className = "";
        private List<AbstractRegionType> regionList = new List<AbstractRegionType>();


        private int RemoveAllIndent()
        {
            const string reduceMultiSpace = @"[ ]{2,}";
            finalString = Regex.Replace(finalString.Replace("\t", " "), reduceMultiSpace, " ");
            return 0;
        }

        private int AutoIndentFinalString()
        {
            RemoveAllIndent();
            string[] temp = finalString.Split('\n');
            finalString = "";
            int ind = 0;
            for (int i = 0; i < temp.Length; i++)
            {
                int opening = temp[i].Where(c => c == '{').Count();
                int closing = temp[i].Where(c => c == '}').Count();
                int diff = opening - closing;
                if (diff < 0)
                {
                    ind += diff;
                }
                for (int j = 0; j < ind; j++)
                {
                    temp[i] = "\t" + temp[i];
                }
                if (diff > 0)
                {
                    ind += diff;
                }
                finalString += temp[i] + "\n";
            }

            return 0;
        }

        public int GenerateNewConstantFile(string className)
        {
            this.className = className;
            finalString = HEADER_GENERATED_COMMENT;
            finalString += String.Format(NAMESPACE_DEFINITION_FORMAT, className, 
                String.Format(CLASS_DEFINITION_FORMAT, className, ""));
            return 0;
        }

        public int SaveToFile(string destinationPath)
        {
            if (finalString == "")
            {
                return -1;
            }
            if (!destinationPath.EndsWith(".cs"))
            {
                return -2;
            }
            if (!File.Exists(destinationPath))
            {
                File.Create(destinationPath);
            }
            RemoveAllIndent();

            string regionsString = RegionListToString();

            string test = classBodyRegex.Replace(finalString, "$1\n" + regionsString + "\n$4");
            finalString = test;
            AutoIndentFinalString();
            File.WriteAllText(destinationPath, finalString);
            return 0;
        }

        public int AddRegion(string _regionName, Rectangle _rec, RegionType _type = RegionType.OverrrideRectangle)
        {
            _regionName = _regionName.ToUpper();
            foreach (var item in regionList)
            {
                if (item.PName == _regionName)
                {
                    return -2;
                }
            }
            if (_regionName.Length < 1)
            {
                return -1;
            }
            regionList.Add(AbstractRegionType.NewRegion(_type, _regionName, _rec));
            return 0;
        }

        public string RegionListToString()
        {
            string retString = "";

            regionList = regionList.OrderBy(x => x.PName).ToList();

            foreach (var item in regionList)
            {
                retString += item.ToString() + "\n";
            }

            return retString;
        }

        public int LoadConstantsFile(string filePath)
        {
            regionList.Clear();
            StreamReader rdr = File.OpenText(filePath);
            finalString = rdr.ReadToEnd();
            File.WriteAllText(filePath + ".cpy", finalString);
            RemoveAllIndent();
            Regex bodyReg = new Regex(@"#region((?s).*?)*?.*?#endregion");
            MatchCollection matches = bodyReg.Matches(finalString);
            foreach (Match item in matches)
            {
                regionList.Add(AbstractRegionType.Parse(item.Value));
            }
            rdr.Close();

            return 0;
        }

        public List<AbstractRegionType> GetRegionList()
        {
            return regionList;
        }

        public int DeleteRegion(string itemName)
        {
            var item = regionList.Find(x => x.PName == itemName);
            regionList.Remove(item);
            return 0;
        }

        public int UpdateRegion(string itemName, Rectangle rec, string newName)
        {
            var item = regionList.Find(x => x.PName == itemName);
            item.PValues = rec;
            item.PName = newName;
            return 0;
        }

        public void SetType(RegionType _type)
        {
            for (int i = 0; i < regionList.Count; i++)
            {
                regionList[i] = AbstractRegionType.NewRegion(_type, regionList[i].PName, regionList[i].PValues);
            }
        }
    }
}
