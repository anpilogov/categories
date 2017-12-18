using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using lingvo.tokenizing;

namespace CategorieaApi.Service
{
    public class Config
    {
		static Config()
		{
			var lst = new List<string>
			{
				"Авто",
				"Экономика и бизнес",
				"Шоу-бизнес и развлечения",
				"Семья",
				"Мода",
				"Компьютерные игры",
				"Здоровье и медицина",
				"Политика",
				"Недвижимость",
				"Наука и технологи",
				"Спорт",
				"Туризм, путешевствия",
				"Кулинария"
			};
			for (var i = 0; ; i++)
			{
				var value = ConfigurationManager.AppSettings["CLASS_INDEX_" + i];
				if (string.IsNullOrWhiteSpace(value))
					break;
				lst.Add(value);
			}
			CLASS_INDEX_NAMES = lst.ToArray();


			var model_folder = AppDomain.CurrentDomain.BaseDirectory + "App_Data\\[resources-4-appharbor.com]";
			var model_filenames = "classify-model-(tfidf-3-1)--1.txt; classify-model-(tfidf-3-1)--2.txt;classify-model-(tfidf-3-1)--3.txt";

			MODEL_FILENAMES = (from raw_model_filename in model_filenames.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
							   let model_filename = raw_model_filename.Trim()
							   let full_model_filename = Path.Combine(model_folder, model_filename)
							   select full_model_filename
							  ).ToArray();
		}

		public static readonly NumberFormatInfo NFI = new NumberFormatInfo() { NumberDecimalSeparator = "." };
		public static readonly string N2 = "N2";
		
		public static readonly string[] MODEL_FILENAMES = new []{"classify-model-(tfidf-3-1)--1.txt", "classify-model-(tfidf-3-1)--2.txt", "classify-model-(tfidf-3-1)--3.txt"};
		public static readonly NGramsType MODEL_NGRAMS_TYPE = (NGramsType)Enum.Parse(typeof(NGramsType), "NGram_3", true);
		public static readonly int MODEL_ROW_CAPACITY = 1100000;
		public static readonly int CLASS_THRESHOLD_PERCENT = 10;
		public static readonly string[] CLASS_INDEX_NAMES;
		
		public static readonly string URL_DETECTOR_RESOURCES_XML_FILENAME = AppDomain.CurrentDomain.BaseDirectory+"App_Data\\url-detector-resources.xml";
        public static readonly string SENT_SPLITTER_RESOURCES_XML_FILENAME = AppDomain.CurrentDomain.BaseDirectory + "App_Data\\sent-splitter-resources.xml";
        public static readonly string TOKENIZER_RESOURCES_XML_FILENAME = AppDomain.CurrentDomain.BaseDirectory + "App_Data\\crfsuite-tokenizer-resources.xml";
        public static readonly string NER_MODEL_FILENAME = AppDomain.CurrentDomain.BaseDirectory + "App_Data\\model_pa_(minfreq-1)_ru";
        public static readonly string NER_TEMPLATE_FILENAME = AppDomain.CurrentDomain.BaseDirectory + "App_Data\\templateNER.txt";
        public static readonly LanguageTypeEnum LANGUAGE_TYPE = LanguageTypeEnum.Ru;

        public static readonly int MAX_INPUTTEXT_LENGTH = 10000;
        public static readonly int CONCURRENT_FACTORY_INSTANCE_COUNT = 2;
        public static readonly int SAME_IP_INTERVAL_REQUEST_IN_SECONDS = 10;
        public static readonly int SAME_IP_MAX_REQUEST_IN_INTERVAL = 3;
        public static readonly int SAME_IP_BANNED_INTERVAL_IN_SECONDS = 120;
    }
}