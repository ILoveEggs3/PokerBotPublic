using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsInputDLL
{
    public interface IKeyboardController
    {
        void InputString(string _str);
        Task InputStringAsync(string _str);
    }
}
