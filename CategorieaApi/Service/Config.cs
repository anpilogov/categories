
using System;
using lingvo.tokenizing;

namespace CategorieaApi.Service
{
    public class Config
    {
        public static readonly string URL_DETECTOR_RESOURCES_XML_FILENAME = AppDomain.CurrentDomain.BaseDirectory+"/App_Data/url-detector-resources.xml";
        public static readonly string SENT_SPLITTER_RESOURCES_XML_FILENAME = AppDomain.CurrentDomain.BaseDirectory + "/App_Data/sent-splitter-resources.xml";
        public static readonly string TOKENIZER_RESOURCES_XML_FILENAME = AppDomain.CurrentDomain.BaseDirectory + "/App_Data/crfsuite-tokenizer-resources.xml";
        public static readonly string NER_MODEL_FILENAME = AppDomain.CurrentDomain.BaseDirectory + "/App_Data/model_pa_(minfreq-1)_ru";
        public static readonly string NER_TEMPLATE_FILENAME = AppDomain.CurrentDomain.BaseDirectory + "/App_Data/templateNER.txt";
        public static readonly LanguageTypeEnum LANGUAGE_TYPE = LanguageTypeEnum.Ru;

        public static readonly int MAX_INPUTTEXT_LENGTH = 10000;
        public static readonly int CONCURRENT_FACTORY_INSTANCE_COUNT = 2;
        public static readonly int SAME_IP_INTERVAL_REQUEST_IN_SECONDS = 10;
        public static readonly int SAME_IP_MAX_REQUEST_IN_INTERVAL = 3;
        public static readonly int SAME_IP_BANNED_INTERVAL_IN_SECONDS = 120;
    }
}