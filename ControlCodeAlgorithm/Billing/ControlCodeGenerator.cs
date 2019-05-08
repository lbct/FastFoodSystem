using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace FastFoodSystem.Scripts.Billing
{
    class ControlCodeGenerator
    {
        private static int[][] Mul = new int[][]
        {
             new int[] {0,1,2,3,4,5,6,7,8,9},
             new int[] {1,2,3,4,0,6,7,8,9,5},
             new int[] {2,3,4,0,1,7,8,9,5,6},
             new int[] {3,4,0,1,2,8,9,5,6,7},
             new int[] {4,0,1,2,3,9,5,6,7,8},
             new int[] {5,9,8,7,6,0,4,3,2,1},
             new int[] {6,5,9,8,7,1,0,4,3,2},
             new int[] {7,6,5,9,8,2,1,0,4,3},
             new int[] {8,7,6,5,9,3,2,1,0,4},
             new int[] {9,8,7,6,5,4,3,2,1,0}
        };
        private static int[][] Per = new int[][]
        {
            new int[] {0,1,2,3,4,5,6,7,8,9},
            new int[] {1,5,7,6,2,8,3,0,9,4},
            new int[] {5,8,0,3,7,9,6,1,4,2},
            new int[] {8,9,1,6,0,4,3,5,2,7},
            new int[] {9,4,5,3,1,2,6,8,7,0},
            new int[] {4,2,8,6,5,7,3,9,0,1},
            new int[] {2,7,9,3,8,0,6,4,1,5},
            new int[] {7,0,4,6,9,1,3,2,5,8}
        };

        private static int[] Inv = new int[] { 0, 4, 3, 2, 1, 5, 6, 7, 8, 9 };

        public static string Generate(string billNumber, string clientNit, DateTime saleDateTime, decimal totalValue, string dosificationCode, string authorizationCode)
        {
            BigInteger sum = new BigInteger(0);

            BigInteger billNumberInt = VerhoeffConcatenation(billNumber, 2);
            BigInteger clientNitInt = VerhoeffConcatenation(clientNit, 2);
            BigInteger saleDateTimeInt = VerhoeffConcatenation(saleDateTime.ToString("yyyyMMdd"), 2);
            BigInteger totalValueInt = VerhoeffConcatenation(Math.Round(totalValue, 0, MidpointRounding.AwayFromZero).ToString(), 2);
            
            sum += billNumberInt;
            sum += clientNitInt;
            sum += saleDateTimeInt;
            sum += totalValueInt;

            string fiveVerhoeff = Verhoeff(sum.ToString(), 5).ToString();
            List<string> splitCode = new List<string>();
            string[] sumValues = new string[]
            {
                authorizationCode,
                billNumberInt.ToString(),
                clientNitInt.ToString(),
                saleDateTimeInt.ToString(),
                totalValueInt.ToString()
            };
            string dosCode = dosificationCode;
            for(int i=0;i<5; i++)
            {
                int limit = (fiveVerhoeff[i] - '0') + 1;
                splitCode.Add(sumValues[i] + dosCode.Substring(0, limit));
                dosCode = dosCode.Substring(limit);
            }
            string msg = string.Join("", splitCode.ToArray());
            string key = dosificationCode + fiveVerhoeff;
            string encMsg = AllegedRC4(msg, key).Replace("-", "");
            int sumEnc = encMsg.Sum(cad => cad);
            int totalEncSum = 0;
            for(int i = 0; i < 5; i++)
            {
                int partialSum = 0;
                for(int j = i; j < encMsg.Length; j += 5)
                {
                    partialSum += encMsg[j];
                }
                totalEncSum += (partialSum * sumEnc) / (fiveVerhoeff[i] - '0' + 1);
            }
            string controlCode = AllegedRC4(Base64(totalEncSum), dosificationCode + fiveVerhoeff);
            return controlCode;
        }

        private static string AllegedRC4(string mensaje, string key)
        {
            string mensajeCifrado = "";
            int x = 0, y = 0, index1 = 0, index2 = 0, nMen;
            int[] state = new int[256];
            for (int i = 0; i < 256; i++)
                state[i] = i;
            for (int i = 0; i < 256; i++)
            {
                index2 = (((int)key[index1]) + state[i] + index2) % 256;
                int aux = state[i];
                state[i] = state[index2];
                state[index2] = aux;
                index1 = (index1 + 1) % key.Length;
            }
            for (int i = 0; i < mensaje.Length; i++)
            {
                x = (x + 1) % 256;
                y = (state[x] + y) % 256;
                int aux = state[x];
                state[x] = state[y];
                state[y] = aux;
                nMen = ((int)mensaje[i]) ^ state[(state[x] + state[y]) % 256];
                string hex = nMen.ToString("X");
                mensajeCifrado = mensajeCifrado + "-" + (hex.Length == 1 ? "0" : "") + hex;
            }
            return mensajeCifrado.Substring(1);
        }

        private static int GetNumber(string code)
        {
            int check = 0;
            for (int i = 0, j=code.Length - 1; j >= 0; i++, j--)
            {
                int digito = (code[j] - '0');
                check = Mul[check][Per[((i + 1) % 8)][digito]];
            }
            return Inv[check];
        }

        private static string Base64(int numero)
        {
            char[] diccionario =  {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
                                   'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J',
                                   'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T',
                                   'U', 'V', 'W', 'X', 'Y', 'Z', 'a', 'b', 'c', 'd',
                                   'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n',
                                   'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x',
                                   'y', 'z', '+', '/' };
            int cociente = 1, resto;
            string palabra = "";
            while (cociente > 0)
            {
                cociente = numero / 64;
                resto = numero % 64;
                palabra = diccionario[resto] + palabra;
                numero = cociente;
            }
            return palabra;
        }

        private static BigInteger VerhoeffConcatenation(string number, int times)
        {
            StringBuilder code = new StringBuilder(number);
            for (int i = 0; i < times; i++)
            {
                int res = GetNumber(code.ToString());
                code.Append(res);
            }
            return BigInteger.Parse(code.ToString());
        }

        private static BigInteger Verhoeff(string number, int times)
        {
            StringBuilder finalRes = new StringBuilder("");
            StringBuilder code = new StringBuilder(number);
            for (int i = 0; i < times; i++)
            {
                int res = GetNumber(code.ToString());
                code.Append(res);
                finalRes.Append(res);
            }
            return BigInteger.Parse(finalRes.ToString());
        }
    }
}
