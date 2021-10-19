using System;
using System.IO;

namespace Carmen.CastingEngine.Allocation
{
    public class StringPersistence : IDataPersistence
    {
        Func<string> getter;
        Action<string> setter;

        public StringPersistence(Func<string> getter, Action<string> setter)
        {
            this.getter = getter;
            this.setter = setter;
        }

        public TextReader Load() => new StringReader(getter());
        public TextWriter Save() => new StringWriterWithCallback(s => setter(s.ToString()));

        private class StringWriterWithCallback : StringWriter
        {
            Action<StringWriter> onDispose;

            public StringWriterWithCallback(Action<StringWriter> on_dispose)
            {
                onDispose = on_dispose;
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                    onDispose(this);
                base.Dispose(disposing);
            }
        }
    }
}
