using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using Api.Models;
using CategorieaApi.Service;
using lingvo.classify;
using lingvo.tokenizing;
using Newtonsoft.Json;

namespace Api.Controllers
{
	public class CategoriesController : ApiController
	{
		// POST api/<controller>
		public Response Post([FromBody]Request request)
		{
			var words = ConcurrentFactoryHelper.GetConcurrentFactory().Run(request.Text, true);
			List<string> teg;
			var text = SendJsonResponse(words, request.Text, true, out teg);
			var names = GetWords(words).Select(__ => new { value = __.Key, type = __.Value }).ToArray();

			var classifyInfos = http_context_data.GetConcurrentFactory().MakeClassify(request.Text);

			string json2 = SendJsonResponse(words, request.Text, false, out teg);
			string json3 = SendJsonResponse(words, request.Text, true);
			var resultTemp = new result(classifyInfos, Config.CLASS_THRESHOLD_PERCENT);
			var categories = resultTemp.classify_infos.Where(__ => (Double.Parse(__.percent.Replace('.',',')) > 10)).Select(__ => __.class_index);
			var result = new Response
			{
				categories = categories.Take(3).ToArray(),
				names = names,
				text = text
			};
			return result;
		}

		private Dictionary<string, string> GetWords(IList<word_t> words)
		{
			var result = new Dictionary<string, string>();

			for (var i = words.Count - 1; 0 <= i; i--)
			{
				var word = $"{words[i].ToString().Substring(3 + words[i].length, words[i].length)}";
				var nerOutputType = words[i].nerOutputType;
				if (!result.ContainsKey(word))
					result.Add(word, nerOutputType.ToString());
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

		private static string ClassIndex2Text(int classIndex)
		{
			if (0 <= classIndex && classIndex < Config.CLASS_INDEX_NAMES.Length)
			{
				return (Config.CLASS_INDEX_NAMES[classIndex]);
			}
			return ("[class-index: " + classIndex + "]");
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
		private struct result
		{
			/// <summary>
			/// 
			/// </summary>
			public struct classify_info
			{
				[JsonProperty(PropertyName = "i")]
				public int class_index
				{
					get;
					set;
				}
				[JsonProperty(PropertyName = "n")]
				public string class_name
				{
					get;
					set;
				}
				[JsonProperty(PropertyName = "p")]
				public string percent
				{
					get;
					set;
				}
			}

			public result(Exception ex) : this()
			{
				exception_message = ex.Message;
			}
			public result(ClassifyInfo[] classifyInfos, double classThresholdPercent) : this()
			{
				if (classifyInfos != null && classifyInfos.Length != 0)
				{
					var sum = classifyInfos.Sum(ci => ci.Cosine);
					classify_infos = (from ci in classifyInfos
									  let percent = (ci.Cosine / sum) * 100
									  where (classThresholdPercent <= percent)
									  select
										new classify_info()
										{
											class_index = ci.ClassIndex,
											class_name = ClassIndex2Text(ci.ClassIndex),
											percent = percent.ToString(Config.N2, Config.NFI),
										}
									 ).ToArray();
				}
			}

			[JsonProperty(PropertyName = "classes")]
			public classify_info[] classify_infos
			{
				get;
				private set;
			}
			[JsonProperty(PropertyName = "err")]
			public string exception_message
			{
				get;
				private set;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		private static class http_context_data
		{
			private static readonly object _SyncLock = new object();

			private static ConcurrentFactory _ConcurrentFactory;

			public static ConcurrentFactory GetConcurrentFactory()
			{
				var f = _ConcurrentFactory;
				if (f == null)
				{
					lock (_SyncLock)
					{
						f = _ConcurrentFactory;
						if (f == null)
						{
							{
								var modelConfig = new ModelConfig()
								{
									Filenames = Config.MODEL_FILENAMES,
									RowCapacity = Config.MODEL_ROW_CAPACITY,
									NGramsType = Config.MODEL_NGRAMS_TYPE
								};
								var model = new ModelNative(modelConfig); //new ModelHalfNative( modelConfig ); //new ModelClassic( modelConfig );
								var config = new ClassifierConfig(Config.URL_DETECTOR_RESOURCES_XML_FILENAME);

								f = new ConcurrentFactory(config, model, Config.CONCURRENT_FACTORY_INSTANCE_COUNT);
								_ConcurrentFactory = f;
							}
							{
								GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
								GC.WaitForPendingFinalizers();
								GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
							}
						}
					}
				}
				return (f);
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