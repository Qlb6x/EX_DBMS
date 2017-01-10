using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace newDBMS
{
    public class TBL
    {
       
        public const int FIELD_NAME_LENGTH=100;
        public struct Field
        {
            //public char[] sFieldNmae = new char[FIELD_NAME_LENGTH];
            //public char[] sType = new char[8];
            public string sFieldName;
            public string sType;
            public int iSize;
            public int bKey;
            public int bNullFlag;
            public int bValidFlag;
        };
        public struct Condition // 保存select中where之后的 详细条件   student.name = lsy
        {
            public string table;
            public string field;
            public string value;
        };
        public struct Detailfield  //保存select 之后要查询的 域   student.name
        {
            public string table;
            public string field;
        };

        public struct TableJilu
        {
            public string table;
            public string field;
            public string[] value;
            public int num_value;//value的个数
            public int indexof;//字段在 dbf文件中的 序列
        };
    }
}
