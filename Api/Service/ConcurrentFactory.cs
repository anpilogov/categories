using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using lingvo.classify;
using lingvo.ner;
using lingvo.tokenizing;

namespace CategorieaApi.Service
{
    public sealed class ConcurrentFactory
    {
        private readonly Semaphore _Semaphore;
        private readonly ConcurrentStack<NerProcessor> _stackNetProcessors;
		private readonly ConcurrentStack<Classifier> _Stack;
		public ConcurrentFactory(ClassifierConfig config, IModel model, int instanceCount)
		{
			if (instanceCount <= 0) throw (new ArgumentException("instanceCount"));
			if (config == null) throw (new ArgumentNullException("config"));
			if (model == null) throw (new ArgumentNullException("model"));

			_Semaphore = new Semaphore(instanceCount, instanceCount);
			_Stack = new ConcurrentStack<Classifier>();
			for (int i = 0; i < instanceCount; i++)
			{
				_Stack.Push(new Classifier(config, model));
			}
		}

		public ClassifyInfo[] MakeClassify(string text)
		{
			_Semaphore.WaitOne();
			var worker = default(Classifier);
			var result = default(ClassifyInfo[]);
			try
			{
				worker = Pop(_Stack);
				result = worker.MakeClassify(text);
			}
			finally
			{
				if (worker != null)
				{
					_Stack.Push(worker);
				}
				_Semaphore.Release();
			}
			return (result);
		}

		public ConcurrentFactory(NerProcessorConfig config, int instanceCount)
        {
            if (instanceCount <= 0) throw (new ArgumentException("instanceCount"));
            if (config == null) throw (new ArgumentNullException("config"));

            _Semaphore = new Semaphore(instanceCount, instanceCount);
            _stackNetProcessors = new ConcurrentStack<NerProcessor>();
            for (int i = 0; i < instanceCount; i++)
            {
                var proc = new NerProcessor(config);
                _stackNetProcessors.Push(proc);
            }
        }

        public word_t[] Run(string text, bool splitBySmiles)
        {
            _Semaphore.WaitOne();
            var worker = default(NerProcessor);
            try
            {
                worker = Pop(_stackNetProcessors);
                if (worker == null)
                {
                    for (var i = 0; ; i++)
                    {
                        worker = Pop(_stackNetProcessors);
                        if (worker != null)
                            break;

                        Thread.Sleep(25);

                        if (10000 <= i)
                            throw (new InvalidOperationException(this.GetType().Name + ": no (fusking) worker item in queue"));
                    }
                }

                var result = worker.Run(text, splitBySmiles).ToArray();
                return (result);
            }
            finally
            {
                if (worker != null)
                {
                    _stackNetProcessors.Push(worker);
                }
                _Semaphore.Release();
            }

            throw (new InvalidOperationException(this.GetType().Name + ": nothing to return (fusking)"));
        }

        private static T Pop<T>(ConcurrentStack<T> stack)
        {
            var t = default(T);
            if (stack.TryPop(out t))
                return (t);
            return (default(T));
        }
    }
}