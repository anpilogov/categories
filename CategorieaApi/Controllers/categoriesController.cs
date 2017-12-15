using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;
using CategorieaApi.Models;
using CategorieaApi.Service;
using lingvo.ner;
using lingvo.tokenizing;
using Newtonsoft.Json;

namespace CategorieaApi.Controllers
{
    public class categoriesController : ApiController
    {
	    private List<string> categories = new List<string> { "NAME", "ORG", "GEO", "ENTR", "PROD" };
        // GET: api/categories
        public IEnumerable<dynamic> Get()
        {
	        var result = categories.Select(__ => new { id = categories.FindIndex(__c => __c == __), name = __ }).ToList();
            return result;
        }

        // GET: api/categories/5
        /*public string Get(int id)
        {
            return "value";
        }*/

        // POST: api/categories
        public Response Post(Request request)
        {
            var words = ConcurrentFactoryHelper.GetConcurrentFactory().Run(request.Text, true);
            List<string> teg;
            var json = SendJsonResponse(words, request.Text, true, out teg);
	        var names = GetWords(words);
	        foreach (var cat in categories)
	        {
		        json = json.Replace(cat, categories.FindIndex(__c => __c == cat).ToString());

	        }
			
            //string json2 = SendJsonResponse(words, request.Text, false, out teg);
            //string json3 = SendJsonResponse(words, request.Text, true);
            var result = new Response
            {
                Categories = teg.Select(__ => categories.FindIndex(__c => __c == __)).ToArray(),
                Names = names.ToArray(),
                Text = json
            };
            return result;
        }
        /*
        // PUT: api/categories/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/categories/5
        public void Delete(int id)
        {
        }*/

	    private List<string> GetWords(IList<word_t> words)
	    {
		    var result = new List<string>();
			for (var i = words.Count - 1; 0 <= i; i--)
			{
				var word = $"{words[i].ToString().Substring(3 + words[i].length, words[i].length)}:{categories.FindIndex(__c => __c == words[i].nerOutputType.ToString())}";
				if (result.Find(__ => __ == word)==null)
					result.Add(word);
				if (result.Count == 3) return result;
			}

		    return result;
	    }
        
        private string SendJsonResponse(IList<word_t> words, string originalText, bool html)
        {
            string result = null;
            if (html)
            {
                result = SendJsonResponse(new result_html(words, originalText));
            }
            return result;
        }
        
        private static string SendJsonResponse(IList<word_t> words, string originalText, bool html, out List<string> teg)
        {
            string result;
            
            if (html)
            {
                //result = SendJsonResponse(new result_html(words, originalText));
                result = SendJsonResponse(new ResultJson(words, originalText, out teg));
            }
            else
            {
                result = SendJsonResponse(new result_json(words));
                teg = new List<string>();
            }
            return result;
        }

        private static string SendJsonResponse(Exception ex)
        {
            return SendJsonResponse(new result_json(ex));
        }
        private static string SendJsonResponse(result_base result)
        {
            var json = JsonConvert.SerializeObject(result);
            return json;
        }
        private static string SendJsonResponse(ResultJson result)
        {
            return result.html;
        }
        /// <summary>
        /// 
        /// </summary>
        private abstract class result_base
        {
            protected result_base()
            {
            }
            protected result_base(Exception ex)
            {
                exceptionMessage = ex.ToString();
            }

            [JsonProperty(PropertyName = "err")]
            public string exceptionMessage
            {
                get;
                private set;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private sealed class result_json : result_base
        {
            /// <summary>
            /// 
            /// </summary>
            public sealed class word_info
            {
                [JsonProperty(PropertyName = "i")]
                public int startIndex
                {
                    get;
                    set;
                }
                [JsonProperty(PropertyName = "l")]
                public int length
                {
                    get;
                    set;
                }
                [JsonProperty(PropertyName = "ner")]
                public string ner
                {
                    get;
                    set;
                }
                [JsonProperty(PropertyName = "v")]
                public string value
                {
                    get;
                    set;
                }
            }

            public result_json(Exception ex) : base(ex)
            {
            }
            public result_json(IList<word_t> _words)
            {
                var word_sb = new StringBuilder();

                words = (from word in _words
                             //let isWordInNerChain = word.IsWordInNerChain
                             //where ( !isWordInNerChain || (isWordInNerChain && word.IsFirstWordInNerChain))
                         where (!word.HasNerPrevWord)
                         select
                             new word_info()
                             {
                                 startIndex = word.startIndex,
                                 length = word.GetNerLength(),
                                 ner = word.nerOutputType.ToString(),
                                 value = word.GetNerValue(word_sb),
                             }
                        ).ToArray();
            }

            public word_info[] words
            {
                get;
                private set;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private sealed class result_html : result_base
        {
            public result_html(Exception ex) : base(ex)
            {
            }
            public result_html(IList<word_t> _words, string originalText)
            {
                var sb = new StringBuilder(originalText);

                for (var i = _words.Count - 1; 0 <= i; i--)
                {
                    var word = _words[i];
                    sb.Insert(word.startIndex + word.length, "</span>");
                    sb.Insert(word.startIndex, string.Format("<span class='{0}'>", word.nerOutputType));
                }

                sb.Replace("\r\n", "<br/>").Replace("\n", "<br/>").Replace("\t", "&nbsp;&nbsp;&nbsp;&nbsp;");

                html = sb.ToString();
            }

            public string html
            {
                get;
                private set;
            }
        }
        private sealed class ResultJson : result_base
        {
            public ResultJson(Exception ex) : base(ex)
            {
            }
            public ResultJson(IList<word_t> _words, string originalText, out List<string> outputType)
            {
                outputType = new List<string>();
                var sb = new StringBuilder(originalText);

                for (var i = _words.Count - 1; 0 <= i; i--)
                {
                    var word = _words[i];
                    sb.Insert(word.startIndex + word.length, $":{word.nerOutputType}>");
                    sb.Insert(word.startIndex, "<");
                    if (outputType.Find(__ => __ == word.nerOutputType.ToString()) == null)
                    {
                        outputType.Add(word.nerOutputType.ToString());
                    }
                }

                //sb.Replace("\r\n", "<br/>").Replace("\n", "<br/>").Replace("\t", "&nbsp;&nbsp;&nbsp;&nbsp;");

                html = sb.ToString();
            }

            public string html
            {
                get;
                private set;
            }
        }
    }
}
