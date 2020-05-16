using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects
{
    public interface IExpert
    {
        string AccountName();
        string Symbol();
        string Comment();

        double Volume();

        long Magic();
    }
}