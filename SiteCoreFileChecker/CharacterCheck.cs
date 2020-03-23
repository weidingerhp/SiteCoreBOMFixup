using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiteCoreFileChecker.Data;

namespace SiteCoreFileChecker {
    public enum BOM_TYPE {
        NO_BOM,
        UTF_8,
        UTF_16LE,
        UTF_16BE,
        UTF_32LE,
        UTF_32BE,
        UTF_7A,
        UTF_7B,
        UTF_1,
        UTF_EBCDIC,
        SCSU,
        BOCU_1,
        GB_18030
    }

    public static class BomExtension {
        public static Encoding ToEncoding(this BOM_TYPE bom) {
            try {
                switch (bom) {
                    case BOM_TYPE.UTF_8:
                        return Encoding.UTF8;
                    case BOM_TYPE.UTF_16LE:
                        return new UnicodeEncoding(false, true);
                    case BOM_TYPE.UTF_16BE:
                        return new UnicodeEncoding(true, true);
                    case BOM_TYPE.UTF_32LE:
                        return new UTF32Encoding(false, true);
                    case BOM_TYPE.UTF_32BE:
                        return new UTF32Encoding(true, true);
                    case BOM_TYPE.UTF_7A:
                    case BOM_TYPE.UTF_7B:
                        return new UTF7Encoding();
                    case BOM_TYPE.UTF_1:
                        return Encoding.Unicode;
                    case BOM_TYPE.UTF_EBCDIC:
                        return Encoding.GetEncoding("IBM037");
                }
            }
            catch (Exception) {
            }

            return Encoding.Default;
        }
    }
    public struct BomCharIndex {
        private int _index;
        private BOM_TYPE _type;

        public BomCharIndex(int index, BOM_TYPE type) {
            _index = index;
            _type = type;
        }

        public int Index {
            get => _index;
        }

        public BOM_TYPE Type {
            get => _type;
        }
    }

    public class CharacterCheck {
        private static readonly int[] FIRSTCHAR = new int[]
            {0x00, 0xFF, 0xEF, 0xFE, 0x2B, 0xF7, 0xDD, 0x0E, 0xFB, 0x84};

        private static readonly int[] UTF7FTHCHAR = new int[] {0x38, 0x39, 0x2B, 0x2F};

        /// <summary>
        /// Checks for BOM chars in various encodings
        /// The BOM can take up to 5 bytes.
        /// <see cref="https://en.wikipedia.org/wiki/Byte_order_mark"/>
        /// </summary>
        /// <param name="fileName">The fileName to check</param>
        /// <param name="fixInPlace">if Set, the function also writes a <filename>.corr - file where the non-ascii-chars are missing</param>
        /// <returns>List of indices in the file where strange chars were found</returns>
        public static async Task<IList<BomCharIndex>> CheckForBOM(string fileName) {
            using (var fileStream = File.Open(fileName, FileMode.Open)) {
                IList<BomCharIndex> resultIndices = new List<BomCharIndex>();
                var bomCandidate = new List<int>();
                int index = 0;

                while (fileStream.CanRead) {
                    var b = fileStream.ReadByte();
                    if (b == -1) break; // EOF Found

                    if (index > 100) break; // dont look the whole file through

                    switch (bomCandidate.Count) {
                        case 0:
                            if (FIRSTCHAR.Any(c => c == b)) bomCandidate.Add(b);
                            break;
                        case 1:
                            Check2ndChar(ref bomCandidate, ref resultIndices, b, index);
                            break;
                        case 2:
                            Check3rdChar(ref bomCandidate, ref resultIndices, b, index);
                            break;
                        case 3:
                            Check4thChar(ref bomCandidate, ref resultIndices, b, index);
                            break;
                        case 4:
                            // This is a 5 or 4 byte UTF7 Bom Char
                            if (b == 0x38) {
                                resultIndices.Add(new BomCharIndex(index - 4, BOM_TYPE.UTF_7B));
                            }
                            else {
                                resultIndices.Add(new BomCharIndex(index - 4, BOM_TYPE.UTF_7A));
                            }

                            bomCandidate.Clear();
                            break;
                    }

                    index++;
                }

                return await Task.FromResult(resultIndices);
            }
        }

        private static void Check2ndChar(ref List<int> bomCandidate, ref IList<BomCharIndex> resultIndices, int b,
            int curIndex) {
            switch (@bomCandidate[0]) {
                case 0xEF:
                    if (b == 0xBB) {
                        // Possible UTF8
                        bomCandidate.Add(b);
                        break;
                    }

                    bomCandidate.Clear();
                    break;
                case 0x00:
                    if (b == 0xB0) {
                        // Possible UTF32BE
                        bomCandidate.Add(b);
                        break;
                    }

                    bomCandidate.Clear();
                    break;
                case 0xFF:
                    if (b == 0xFE) {
                        // Possible UTF32LE or UTF16LE
                        bomCandidate.Add(b);
                        break;
                    }

                    bomCandidate.Clear();
                    break;
                case 0xFE:
                    if (b == 0xFF) {
                        // Found UTF16BE
                        resultIndices.Add(new BomCharIndex(curIndex - 1, BOM_TYPE.UTF_16BE));
                    }

                    bomCandidate.Clear();
                    break;
                case 0x2B:
                    if (UTF7FTHCHAR.Any(c => c == b))
                        bomCandidate.Add(b);
                    else
                        bomCandidate.Clear();
                    break;
                case 0xF7:
                    if (b == 0x64) {
                        // Possible UTF1
                        bomCandidate.Add(b);
                        break;
                    }

                    bomCandidate.Clear();
                    break;
                case 0xDD:
                    if (b == 0x73) {
                        // Possible UTF-EBCDIC
                        bomCandidate.Add(b);
                        break;
                    }

                    bomCandidate.Clear();
                    break;
                case 0x0E:
                    if (b == 0xFE) {
                        // Possible SCSU
                        bomCandidate.Add(b);
                        break;
                    }

                    bomCandidate.Clear();
                    break;
                case 0xFB:
                    if (b == 0xEE) {
                        // Possible BOCU-1
                        bomCandidate.Add(b);
                        break;
                    }

                    bomCandidate.Clear();
                    break;
                case 0x84:
                    if (b == 0x31) {
                        // Possible GB-18030
                        bomCandidate.Add(b);
                        break;
                    }

                    bomCandidate.Clear();
                    break;
                default:
                    bomCandidate.Clear();
                    break;
            }
        }

        private static void Check3rdChar(ref List<int> bomCandidate, ref IList<BomCharIndex> resultIndices, int b,
            int curIndex) {
            switch (@bomCandidate[0]) {
                case 0xEF:
                    if (b == 0xBF) {
                        // Found UTF8-BOM
                        resultIndices.Add(new BomCharIndex(curIndex - 2, BOM_TYPE.UTF_8));
                    }

                    bomCandidate.Clear();
                    break;
                case 0x00:
                    if (b == 0xFE) {
                        // Possible UTF32BE
                        bomCandidate.Add(b);
                        break;
                    }

                    bomCandidate.Clear();
                    break;
                case 0xFF:
                    if (b == 0x00) {
                        // Possible UTF32LE otherwise its UTF16LE
                        bomCandidate.Add(b);
                        break;
                    }

                    resultIndices.Add(new BomCharIndex(curIndex - 2, BOM_TYPE.UTF_16LE));
                    bomCandidate.Clear();
                    break;
                case 0x2B:
                    if (b == 0x76) {
                        // Possible UTF7
                        bomCandidate.Add(b);
                        break;
                    }

                    bomCandidate.Clear();
                    break;
                case 0xF7:
                    if (b == 0x4C) {
                        // Found UTF1 BOM
                        resultIndices.Add(new BomCharIndex(curIndex - 2, BOM_TYPE.UTF_1));
                    }

                    bomCandidate.Clear();
                    break;
                case 0xDD:
                    if (b == 0x66) {
                        // Possible UTF-EBCDIC
                        bomCandidate.Add(b);
                        break;
                    }

                    bomCandidate.Clear();
                    break;
                case 0x0E:
                    if (b == 0xFF) {
                        // Found SCSU BOM
                        resultIndices.Add(new BomCharIndex(curIndex - 2, BOM_TYPE.SCSU));
                    }

                    bomCandidate.Clear();
                    break;
                case 0xFB:
                    if (b == 0x28) {
                        // Found BOCU-1
                        resultIndices.Add(new BomCharIndex(curIndex - 2, BOM_TYPE.BOCU_1));
                        break;
                    }

                    bomCandidate.Clear();
                    break;
                case 0x84:
                    if (b == 0x95) {
                        // Possible GB-18030
                        bomCandidate.Add(b);
                        break;
                    }

                    bomCandidate.Clear();
                    break;
                default:
                    bomCandidate.Clear();
                    break;
            }
        }

        private static void Check4thChar(ref List<int> bomCandidate, ref IList<BomCharIndex> resultIndices, int b,
            int curIndex) {
            switch (@bomCandidate[0]) {
                case 0x00:
                    if (b == 0xFF) {
                        // Found UTF32BE BOM 
                        resultIndices.Add(new BomCharIndex(curIndex - 3, BOM_TYPE.UTF_32BE));
                    }

                    bomCandidate.Clear();
                    break;
                case 0xFF:
                    if (b == 0x00) {
                        // Found UTF32LE BOM 
                        resultIndices.Add(new BomCharIndex(curIndex - 3, BOM_TYPE.UTF_32LE));
                    }

                    bomCandidate.Clear();
                    break;
                case 0xDD:
                    if (b == 0x73) {
                        // Found EBCDIC-UTF
                        resultIndices.Add(new BomCharIndex(curIndex - 3, BOM_TYPE.UTF_EBCDIC));
                    }

                    bomCandidate.Clear();
                    break;
                case 0x2B:
                    if (b == 0x38) {
                        // Possible UTF7
                        bomCandidate.Add(b);
                        break;
                    }

                    bomCandidate.Clear();
                    break;
                case 0x84:
                    if (b == 0x33) {
                        // Found GB-18030 BOM
                        resultIndices.Add(new BomCharIndex(curIndex - 3, BOM_TYPE.GB_18030));
                    }

                    bomCandidate.Clear();
                    break;
                default:
                    bomCandidate.Clear();
                    break;
            }
        }

        public static async Task<bool> RemoveBOM(string fileName) {
            bool wasFixed = false;
            var bomItems = await CheckForBOM(fileName);
            // create a list of items and indices to remove
            Dictionary<long, int> removeMap = new Dictionary<long, int>();
            GetBomLengths(bomItems, removeMap);

            var tempFile = Path.GetTempFileName();
            using (var output = File.OpenWrite(tempFile)) {

                using (var fileStream = File.Open(fileName, FileMode.Open)) {
                    long index = 0;
                    while (fileStream.CanRead) {
                        var b = fileStream.ReadByte();
                        if (b == -1) break; // EOF Found
                        if (removeMap.ContainsKey(index)) {
                            int count = removeMap[index];
                            for (int j = 0; j < count; j++) {
                                b = fileStream.ReadByte();
                            }

                            index += count;
                            wasFixed = true;
                        }

                        output.WriteByte((byte) b);
                        index++;
                    }
                }
            }

            File.Copy(tempFile, fileName, true);
            return wasFixed;
        }

        private static void GetBomLengths(IList<BomCharIndex> bomItems, Dictionary<long, int> removeMap) {
            foreach (var item in bomItems) {
                switch (item.Type) {
                    case BOM_TYPE.UTF_8:
                        removeMap[item.Index] = 3;
                        break;
                    case BOM_TYPE.NO_BOM:
                        break;
                    case BOM_TYPE.UTF_16LE:
                        break;
                    case BOM_TYPE.UTF_16BE:
                        break;
                    case BOM_TYPE.UTF_32LE:
                        break;
                    case BOM_TYPE.UTF_32BE:
                        break;
                    case BOM_TYPE.UTF_7A:
                        break;
                    case BOM_TYPE.UTF_7B:
                        break;
                    case BOM_TYPE.UTF_1:
                        break;
                    case BOM_TYPE.UTF_EBCDIC:
                        break;
                    case BOM_TYPE.SCSU:
                        break;
                    case BOM_TYPE.BOCU_1:
                        break;
                    case BOM_TYPE.GB_18030:
                        break;
                }
            }
        }
    }
}