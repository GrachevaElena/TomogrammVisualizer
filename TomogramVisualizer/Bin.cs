using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace TomogramVisualizer
{
    public class Bin
    {
        public static int X, Y, Z;
        public static short[,,] array;

        public static void ReadBin(string path)
        {
            if (File.Exists(path))
            {
                BinaryReader reader = new BinaryReader(File.Open(path, FileMode.Open));

                X = reader.ReadInt32();
                Y = reader.ReadInt32();
                Z = reader.ReadInt32();

                array = new short[X, Y, Z];
                for (int k = 0; k < Z; k++)
                    for (int j = 0; j < Y; j++)
                        for (int i = 0; i < X; i++)
                        {
                            array[i, j, k] = reader.ReadInt16();
                        }
            }
        }
    }
}
