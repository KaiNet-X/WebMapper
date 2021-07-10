using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KaiNetWebMapper
{
    public class Url
    {
        public readonly string Base;

        private string fullDomain;
        public string FullDomain 
        { 
            get
            {
                if (fullDomain == null)
                {
                    if (!Base.Contains("http") && !Base.StartsWith("/") && !Base.StartsWith("~")) fullDomain =  "";
                    else
                    {
                        string str = Base.Replace("http://", "").Replace("https://", "").Replace("www.", "").Replace("www2.", "");
                        string[] arr = str.Split('/');

                        if (arr.Length == 0) fullDomain = "";
                        else
                        {
                            fullDomain = arr[0];
                        }
                    }
                }
                return fullDomain;
            } 
        }

        private string domain;
        public string Domain
        {
            get
            {
                if (domain == null)
                {
                    if (fullDomain == "") domain = "";
                    else
                    {
                        string[] arr = FullDomain.Split('.');
                        if (arr.Length < 2) domain = "";
                        else domain = arr[arr.Length - 2];
                    }
                }
                return domain;
            }
        }


        private string path;
        public string Path
        {
            get
            {
                if (path == null)
                {
                    if (!Base.Contains("http") && !Base.StartsWith("/") && !Base.StartsWith("~")) path = "";
                    else
                    {
                        string str = Base.Replace("//", "");
                        int index = str.IndexOf("/");
                        str = str.Substring(0, str.IndexOf('?'));
                        if (index == -1) path = "";
                        else path = str.Substring(index + 1);
                    }
                }
                return path;
            }
        }

        private string[] segments;
        public string[] Segments
        {
            get
            {
                if (segments == null)
                {
                    if (Path == "") segments = new string[0];
                    else
                    {
                        segments = path.Split('/');
                    }
                }
                return segments;
            }
        }

        private string[] queryStrings;
        public string[] QueryStrings
        {
            get
            {
                if (queryStrings == null)
                {
                    if (!Base.Contains('?')) queryStrings = new string[0];
                    else
                    {
                        queryStrings = Base.Substring(Base.IndexOf('?') + 1).Split('?');
                    }
                }
                return queryStrings;
            }
        }

        public Url(string url)
        {
            Base = url;
        }
    }
}