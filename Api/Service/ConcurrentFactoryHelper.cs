using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using lingvo.ner;
using lingvo.sentsplitting;

namespace CategorieaApi.Service
{
    public static class ConcurrentFactoryHelper
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
                        var sentSplitterConfig = new SentSplitterConfig(Config.SENT_SPLITTER_RESOURCES_XML_FILENAME,
                                                                         Config.URL_DETECTOR_RESOURCES_XML_FILENAME);
                        var config = new NerProcessorConfig(Config.TOKENIZER_RESOURCES_XML_FILENAME,
                                                             Config.LANGUAGE_TYPE,
                                                             sentSplitterConfig)
                        {
                            ModelFilename = Config.NER_MODEL_FILENAME,
                            TemplateFilename = Config.NER_TEMPLATE_FILENAME,
                        };
                        f = new ConcurrentFactory(config, Config.CONCURRENT_FACTORY_INSTANCE_COUNT);
                        _ConcurrentFactory = f;
                    }
                }
            }
            return (f);
        }
    }
}