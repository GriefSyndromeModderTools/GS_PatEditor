using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GS_PatEditor.Editor.Exporters.CodeFormat
{
    public interface ILineObject
    {
        void Write(TextWriter output, int indent);
    }

    public class SimpleLineObject : ILineObject
    {
        private readonly string _Value;

        public static readonly SimpleLineObject Empty = new SimpleLineObject("");

        public SimpleLineObject(string val)
        {
            _Value = val;
        }

        public void Write(TextWriter output, int indent)
        {
            if (_Value != null && _Value.Length > 0)
            {
                output.WriteIndent(indent);
                output.WriteLine(_Value);
            }
        }
    }

}
