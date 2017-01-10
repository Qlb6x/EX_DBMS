using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
namespace newDBMS
{
    public partial class Form1 : Form
    {
        public const int MAX_NUM = 200;
        public string path = new DirectoryInfo("../../../").FullName + "database/";
        public const int num_para = 500;
        public TBL.Field[] newtable = new TBL.Field[MAX_NUM];
        public TBL.Field[] searchtable = new TBL.Field[MAX_NUM];


        public int NUM_jilu = 0;//配合searchtable使用
        public int NUM_ziduan = 0;
        public int num_whichfield = 0;//在dbf中找到field 的序号
        public string[] parameters = new string[num_para];
        public string database_name;




        public string[] table_select_where = new string[MAX_NUM];//from之后有几个 表  student,book
        public TBL.Condition[] condition = new TBL.Condition[MAX_NUM]; //where之后的条件 student.name=lsy 
        public TBL.Detailfield[] detailfield = new TBL.Detailfield[MAX_NUM]; //select之后要查询的属性  student.name

        public TBL.TableJilu[][] tablejilu = new TBL.TableJilu[MAX_NUM][];


        public void clearall()
        {
            for (int i = 0; i < MAX_NUM; i++)
            {
                table_select_where[i] = null;
                condition[i].table = null;
                condition[i].field = null;
                condition[i].value = null;
                detailfield[i].table = null;
                detailfield[i].field = null;
                for (int j = 0; j < MAX_NUM; j++)
                {
                    tablejilu[i] = new TBL.TableJilu[MAX_NUM];
                    tablejilu[i][j].table = null;
                    tablejilu[i][j].field = null;
                    tablejilu[i][j].value = new string[100];
                    //tablejilu[i][j].value = null;
                    tablejilu[i][j].indexof = -1;
                    tablejilu[i][j].num_value = 0;
                }
            }
        }
        public int searchindex(string database, string table, string field) //用于查找某个 字段 在dbf中的序号
        {
            FileStream fs = new FileStream(path + database + ".dbf", FileMode.Open, FileAccess.Read);
            BinaryReader bw = new BinaryReader(fs);
            while (bw.PeekChar() != -1)
            {
                if (bw.ReadString() == "~")
                {
                    if (bw.ReadString() == table)
                    {

                        int time = int.Parse(bw.ReadString());

                        for (int i = 0; i < time; i++)
                        {

                            if (bw.ReadString() == field)
                            {
                                return i;
                            }
                            bw.ReadString();
                            bw.ReadString();
                            bw.ReadString();
                            bw.ReadString();
                            bw.ReadString();
                        }
                    }
                }
            }
            fs.Close();
            bw.Close();
            return -1;
        }
        public void madetablejilu(string database, string table, int num) //num代表是第几个表,把selectfrom之后出现的表 的 所有属性(field)都 保存下来
        {
            searchfor(database, table);
            for (int i = 0; i < MAX_NUM; i++)
            {
                tablejilu[num][i].table = table;
                tablejilu[num][i].field = searchtable[i].sFieldName;
                tablejilu[num][i].indexof = searchindex(database_name, table, tablejilu[num][i].field);
            }
            MessageBox.Show("!");
        }
        public int chaifen(string s)
        {
            int i = 0, k = 0; ;
            for (i = 0; i < num_para; i++)
            {
                parameters[i] = null;
            }
            i = 0;
            try
            {
                while (s[k] != ';')
                {

                    while (s[k] == ' ' || s[k] == '\n')
                    {
                        k++;
                    }
                    while (s[k] != ' ' && s[k] != '\n')
                    {
                        if (s[k] == ',' && s[k - 1] != ' ')
                        {
                            i++;
                            parameters[i] += ",";
                            i++;

                            continue;
                        }
                        parameters[i] += s[k].ToString();
                        k++;
                    }
                    i++; k++;
                }
                parameters[i] = ";";
                return 1;
            }
            catch (Exception)
            {
                MessageBox.Show("请输入规范的sql语句!");
                for (i = 0; i < num_para; i++)
                {
                    parameters[i] = null;
                }
                return 0;
            }

        }
        public void analyze()
        {
            for (int i = 0; parameters[i] != null; i++) //找到database_name
            {
                if (parameters[i] == "in")
                {
                    database_name = parameters[i + 1];
                }
            }
            if (parameters[0] == "create") //如果是DLL中的create
            {
                if (DDL_create() == 1)

                    MessageBox.Show("创建表成功!");

                else

                    MessageBox.Show("创建表失败!");
            }
            if (parameters[0] == "insert")
            {
                if (DML_insert() == 1)
                    MessageBox.Show("插入成功!");


                else
                    MessageBox.Show("插入失败!");
            }
            if (parameters[0] == "edit")
            {
                if (DDL_edit() == 1)
                    MessageBox.Show("修改表成功!");
                else
                    MessageBox.Show("修改表失败!");
            }
            if (parameters[0] == "rename")
            {
                if (DDL_rename() == 1)
                    MessageBox.Show("更改表名成功!");
                else
                    MessageBox.Show("修改表名失败!");
            }
            if (parameters[0] == "drop")
            {
                if (DDL_drop() == 1)
                    MessageBox.Show("删除表成功!");
                else
                    MessageBox.Show("删除表失败!");
            }
            if (parameters[0] == "delete")
            {
                if (DDL_delete() == 1)
                    MessageBox.Show("删除记录成功!");
                else
                    MessageBox.Show("删除记录失败!");
            }
            if (parameters[0] == "update")
            {
                if (DML_update() == 1)
                    MessageBox.Show("修改记录成功!");
                else
                    MessageBox.Show("没有找到相应的记录，影响行数0!");
            }
            if (parameters[0] == "select")
            {
                //清空一个数组，和两个结构体
                clearall();

                int temp = 0;
                //找出from后面的表格 存入table_select_where
                for (int i = 0; parameters[i] != null; i++)
                {
                    if (parameters[i] == "from")
                    {
                        for (int j = i + 1; parameters[j] != null; j++)
                        {
                            if (parameters[j] == "where" || parameters[j] == "in")
                                break;
                            if (parameters[j] != "," && parameters[j] != ";" && parameters[j] != "in")
                            {
                                table_select_where[temp] = parameters[j];

                                temp++;
                            }
                        }
                        break;
                    }
                }
                temp = 0;
                //拆分select 之后的 域
                for (int i = 1; parameters[i - 1] != "from"; i = i + 2)
                {
                    if (parameters[i] == "*")
                    {
                        temp = 0;
                        for (int j = 0; table_select_where[j] != null; j++)
                        {

                            searchfor(database_name, table_select_where[j]);

                            for (int k = 0; searchtable[k].sFieldName != null; k++)
                            {
                                detailfield[temp].table = table_select_where[j];
                                detailfield[temp].field = searchtable[k].sFieldName;
                                temp++;
                            }
                        }
                        break;
                    }
                    if (parameters[i].Contains('.') == false)
                    {
                        detailfield[temp++].field = parameters[i];
                    }
                    else
                    {
                        detailfield[temp].table = parameters[i].Substring(0, parameters[i].IndexOf("."));
                        detailfield[temp].field = parameters[i].Substring(parameters[i].IndexOf(".") + 1);
                        temp++;
                    }
                }
                temp = 0;
                //拆分where之后的 condition
                for (int i = 0; parameters[i] != ";"; i++)
                {
                    if (parameters[i] == "where")
                    {
                        for (int j = i + 1; parameters[j] != null; j = j + 4)
                        {
                            if (parameters[j].Contains(".") == false)
                            {
                                condition[temp++].field = parameters[j];
                            }
                            else
                            {
                                condition[temp].table = parameters[j].Substring(0, parameters[j].IndexOf('.'));
                                condition[temp].field = parameters[j].Substring(parameters[j].IndexOf('.') + 1);
                                condition[temp].value = parameters[j + 2];
                                temp++;
                            }
                        }
                        break;
                    }
                }

                if (select() == 1)
                    MessageBox.Show("查询成功!");
                else
                    MessageBox.Show("查询失败!");


            }

        }
        public int select()
        {
            //清空tablejilu
            for (int i = 0; i < MAX_NUM; i++)
            {

                for (int j = 0; j < MAX_NUM; j++)
                {
                    tablejilu[i] = new TBL.TableJilu[MAX_NUM];
                    tablejilu[i][j].table = null;
                    tablejilu[i][j].field = null;
                    tablejilu[i][j].value = null;
                    tablejilu[i][j].indexof = -1;
                }
            }
            //进入 select()之前，detialfield数组已经把 select之后的 字段处理好了,是*还是个别的属性

            for (int i = 0; table_select_where[i] != null; i++)//把记录结构体 的 table和field两个字符串填充好，还差value
            {
                madetablejilu(database_name, table_select_where[i], i);
            }
            //打印 抬头 在listview3上
            listView3.Clear();
            listView3.View = View.Details;
            for (int i = 0; detailfield[i].field != null; i++)
            {
                listView3.Columns.Add(detailfield[i].table + "." + detailfield[i].field, 110, HorizontalAlignment.Center);
            }
            //遍历第一次数据全部存入 tablejilu 的value[]
            for (int x = 0; table_select_where[x] != null; x++)
            {
                string condition_value = null;
                string condition_field = null;
                string condition_table = null;
                int iIndex = -1;//记录 在tablejilu中的顺序
                if (condition[0].field == null)
                {
                    condition_table = table_select_where[x];
                }
                for (int y = 0; condition[y].field != null; y++)
                {
                    if (table_select_where[x] == condition[y].table)
                    {
                        condition_value = condition[y].value;
                        condition_table = table_select_where[x];
                        condition_field = condition[y].field;
                        break;
                    }
                }
                //找table_index 
                for (int y = 0; tablejilu[y][0].table != null; y++)
                {
                    if (tablejilu[y][0].table == condition_table)
                    {
                        iIndex = y;
                        break;
                    }
                }
                FileStream fs = new FileStream(path + database_name + ".dat", FileMode.Open, FileAccess.Read);
                BinaryReader bw = new BinaryReader(fs);
                searchfor(database_name, condition_table);//condition_table代表的表的所有字段保存 在searchtable 
                while (bw.PeekChar() != -1)
                {
                    if (bw.ReadString() == "~")
                    {
                        if (bw.ReadString() == condition_table)
                        {
                            int num_jilu = int.Parse(bw.ReadString());
                            int num_ziduan = int.Parse(bw.ReadString());
                            for (int m = 0; m < num_ziduan; m++)
                            {
                                tablejilu[iIndex][m].value = new string[200];
                            }
                            for (int n = 0; n < num_jilu; n++)
                            {
                                for (int m = 0; m < num_ziduan; m++)
                                {

                                    string str = bw.ReadString();
                                    for (int v = 0; v < MAX_NUM; v++)
                                    {
                                        if (tablejilu[iIndex][m].value[v] == null)
                                        {
                                            tablejilu[iIndex][m].value[v] = str;
                                            tablejilu[iIndex][m].num_value++;

                                            break;
                                        }
                                    }

                                }
                            }
                        }
                    }
                }
                fs.Close();
                bw.Close();
            }
            //开始打印到richbox3
            if (table_select_where[1] != null)
            {
                for (int i = 0; i < tablejilu[0][0].num_value; i++)
                {
                    for (int j = 0; j < tablejilu[1][0].num_value; j++)
                    {
                        int x = searchindex(database_name, detailfield[i].table, detailfield[i].field);
                        int y = searchindex(database_name, detailfield[j].table, detailfield[j].field);
                        ListViewItem lvi = new ListViewItem();

                        //for (int i = 0; i < 10; i++)
                        //{
                        //    ListViewItem lvi = new ListViewItem();
                        //    lvi.Text = "1列";
                        //    lvi.SubItems.Add("2列第" + i);
                        //    lvi.SubItems.Add("3列第" + i);
                        //    this.listView1.Items.Add(lvi);
                        //}
                        //MessageBox.Show(name_database + "  " + name_table);
                    }
                }
            }
            else //单表查询
            {
                int num_value = 0;
                for (int i = 0; tablejilu[0][i].field != null; i++)
                {
                    if (tablejilu[0][i].num_value >= 0)
                    {
                        num_value = tablejilu[0][i].num_value;
                        break;
                    }
                }
                for (int x = 0; x < num_value; x++)
                {
                    ListViewItem lvi = new ListViewItem();
                    int flag = 0;
                    int FLAG = 1;//是否 通过了where
                    if(condition[0].field==null)
                    {
                        FLAG = 1;//没有where
                    }
                    for (int i = 0; detailfield[i].field != null; i++)
                    {
                        searchfor(database_name, detailfield[i].table);
                        for (int j = 0; searchtable[j].sFieldName != null; j++)
                        {
                            if (searchtable[j].sFieldName == detailfield[i].field)
                            {
                                if (flag == 0)
                                {
                                    lvi.Text = tablejilu[0][j].value[x];
                                    flag = 1;
                                }
                                else
                                {
                                    lvi.SubItems.Add(tablejilu[0][j].value[x]);

                                }

                                for (int p = 0; tablejilu[0][p].field != null; p++)
                                {
                                    for (int y = 0; condition[y].field != null; y++)
                                    {
                                        if (condition[y].field == tablejilu[0][p].field)
                                        {
                                            if (condition[y].value != tablejilu[0][p].value[x])
                                                FLAG = 0;
                                        }
                                    }
                                }
                                break;
                            }

                        }
                    }
                    if (FLAG == 1)
                    {
                        this.listView3.Items.Add(lvi);
                    }
                }
            }
            return 1;
        }
        public int DML_update()
        {
            try
            {

                string name_table = parameters[1];  //
                string field_qian = parameters[4]; //sex
                string value_qian = parameters[6]; //0
                string field_hou = parameters[8]; //name
                string value_hou = parameters[10];//lsy
                searchfor(database_name, name_table);//存储 field在searchtable中
                //寻找 field_qian和field_hou的序号
                int xuhao_qian = -1;
                int xuhao_hou = -1;
                FileStream f1 = new FileStream(path + database_name + ".dbf", FileMode.Open, FileAccess.Read);
                BinaryReader b1 = new BinaryReader(f1);
                while (b1.PeekChar() != -1)
                {
                    if (b1.ReadString() == "~")
                    {
                        if (b1.ReadString() == name_table)
                        {
                            int time = int.Parse(b1.ReadString());

                            for (int i = 0; i < time; i++)
                            {
                                string str = b1.ReadString();
                                xuhao_qian = (str == field_qian) ? i : xuhao_qian;
                                xuhao_hou = (str == field_hou) ? i : xuhao_hou;
                                b1.ReadString();
                                b1.ReadString();
                                b1.ReadString();
                                b1.ReadString();
                                b1.ReadString();
                            }
                        }
                    }
                }
                f1.Close();
                b1.Close();

                //MessageBox.Show("qian:" + xuhao_qian + " hou:" + xuhao_hou);
                if (xuhao_qian == -1 || xuhao_hou == -1)
                {
                    return 0;
                }

                //定义两个string数组 ，用于存放 可能会修改的记录，string true用于存放确实修改了的数据,string flase用于没有修改的数据
                FileStream f3 = new FileStream(path + database_name + ".dat", FileMode.Open, FileAccess.Read);
                BinaryReader b3 = new BinaryReader(f3);
                //temp.dat的文件流
                FileStream f4 = new FileStream(path + "temp.dat", FileMode.Create, FileAccess.Write);
                f4.SetLength(0);
                BinaryWriter b4 = new BinaryWriter(f4);
                int FLAG = 0;
                while (b3.PeekChar() != -1)
                {

                    string temp = b3.ReadString();
                    if (temp == "~")
                    {
                        b4.Write(temp);
                        string temp_name_table = b3.ReadString();
                        if (temp_name_table == name_table)
                        {
                            b4.Write(temp_name_table);
                            int num_jilu = int.Parse(b3.ReadString());
                            b4.Write(num_jilu.ToString());
                            int num_ziduan = int.Parse(b3.ReadString());
                            b4.Write(num_ziduan.ToString());

                            for (int i = 0; i < num_jilu; i++)
                            {
                                int flag = 0;//如果为1说明确实要修改，用update_true写入文件
                                string[] update_true = new string[200];
                                string[] update_false = new string[200];

                                for (int j = 0; j < num_ziduan; j++)
                                {
                                    update_false[j] = b3.ReadString();//先把一条记录读下来
                                    update_true[j] = update_false[j];
                                }
                                if (searchtable[xuhao_hou].sFieldName == field_hou && update_false[xuhao_hou] == value_hou)
                                {
                                    if (searchtable[xuhao_qian].sFieldName == field_qian)
                                    {
                                        flag = 1;
                                        FLAG = 1;
                                        update_true[xuhao_qian] = value_qian;
                                    }
                                }

                                if (flag == 0)
                                {
                                    for (int j = 0; j < num_ziduan; j++)
                                        b4.Write(update_false[j]);
                                }
                                else
                                {
                                    for (int j = 0; j < num_ziduan; j++)
                                        b4.Write(update_true[j]);
                                }
                            }

                        }
                        else
                        {
                            b4.Write(temp_name_table);
                        }
                        continue;
                    }
                    b4.Write(temp);
                }
                b3.Close();
                f3.Close();
                f4.Close();
                b4.Close();
                if (FLAG == 0)
                {
                    return 0;
                }
                copyfile("temp.dat", database_name + ".dat");
                return 1;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return 0;
            }
        }
        public void searchfor(string database, string table) //用于查找某个数据库，某张表的tbl,存在searchtable结构体中
        {
            for (int i = 0; i < MAX_NUM; i++)
            {
                searchtable[i].sFieldName = null;
                searchtable[i].sType = null;
            }
            FileStream f1 = new FileStream(path + database + ".dbf", FileMode.Open, FileAccess.Read);
            BinaryReader b1 = new BinaryReader(f1);
            while (b1.PeekChar() != -1)
            {
                if (b1.ReadString() == "~")
                {
                    if (b1.ReadString() == table)
                    {
                        NUM_ziduan = int.Parse(b1.ReadString());
                        for (int i = 0; i < NUM_ziduan; i++)
                        {
                            string t = b1.ReadString();
                            searchtable[i].sFieldName = t;
                            b1.ReadString();
                            b1.ReadString();
                            b1.ReadString();
                            b1.ReadString();
                            b1.ReadString();

                        }
                    }
                }
            }
            f1.Close();
            b1.Close();
        }
        public int DDL_delete()
        {
            string name_table = parameters[2];
            string field = parameters[4];
            string value = parameters[6];
            //去搜索 有多少个 value
            FileStream f1 = new FileStream(path + database_name + ".dat", FileMode.Open, FileAccess.Read);
            BinaryReader b1 = new BinaryReader(f1);
            int num_delete = 0;
            while (b1.PeekChar() != -1)
            {
                if (b1.ReadString() == "~")
                {
                    if (b1.ReadString() == name_table)
                    {
                        searchfor(database_name, name_table);
                        int num_jilu = int.Parse(b1.ReadString());
                        int num_ziduan = int.Parse(b1.ReadString());

                        for (int i = 0; i < num_jilu; i++)
                        {
                            for (int j = 0; j < num_ziduan; j++)
                            {
                                string temp = b1.ReadString();
                                if (field == searchtable[j].sFieldName && value == temp)
                                {
                                    num_whichfield = j;
                                    num_delete++;
                                }
                            }
                        }
                    }
                }
            }
            f1.Close();
            b1.Close();
            //上面计算了要删除的记录个数num_delete ,现在开始处理dat
            FileStream f3 = new FileStream(path + database_name + ".dat", FileMode.Open, FileAccess.Read);
            BinaryReader b3 = new BinaryReader(f3);
            //temp.dat的文件流
            FileStream f4 = new FileStream(path + "temp.dat", FileMode.Create, FileAccess.Write);
            f4.SetLength(0);
            BinaryWriter b4 = new BinaryWriter(f4);
            while (b3.PeekChar() != -1)
            {
                string temp = b3.ReadString();
                if (temp == "~")
                {
                    b4.Write(temp);
                    string temp_name_table = b3.ReadString();
                    if (temp_name_table == name_table)
                    {
                        b4.Write(temp_name_table);
                        int num_jilu = int.Parse(b3.ReadString());
                        int num_ziduan = int.Parse(b3.ReadString());
                        b4.Write((num_jilu - num_delete).ToString());
                        b4.Write(num_ziduan.ToString());
                        //下面进入数据部分
                        for (int i = 0; i < num_jilu; i++)
                        {
                            //MessageBox.Show("jilu " + num_jilu);
                            //MessageBox.Show("次数" + i);
                            int flag = 1;
                            string[] deletetemp = new string[200];
                            for (int j = 0; j < num_ziduan; j++)
                            {
                                deletetemp[j] = b3.ReadString();
                                if (field == searchtable[j].sFieldName && value == deletetemp[j])
                                {
                                    flag = 0;//找到了 要删除的 一行数据
                                    for (int x = 0; x < num_ziduan - 1 - num_whichfield; x++)
                                    {
                                        b3.ReadString();
                                    }
                                    break;
                                }


                            }
                            if (flag == 1)
                            {
                                for (int y = 0; y < num_ziduan; y++)
                                {
                                    b4.Write(deletetemp[y]);
                                }
                            }
                        }
                    }
                    else
                    {
                        b4.Write(temp_name_table);
                    }
                    continue;
                }
                b4.Write(temp);
            }
            f3.Close();
            b3.Close();
            f4.Close();
            b4.Close();
            //覆盖
            copyfile("temp.dat", database_name + ".dat");
            return 1;
        }
        public int DDL_drop()
        {
            try
            {
                string name_table = parameters[2];
                //dbf
                FileStream f1 = new FileStream(path + database_name + ".dbf", FileMode.Open, FileAccess.Read);
                BinaryReader b1 = new BinaryReader(f1);
                //temp.dbf的文件流
                FileStream f2 = new FileStream(path + "temp.dbf", FileMode.Create, FileAccess.Write);
                f2.SetLength(0);
                BinaryWriter b2 = new BinaryWriter(f2);
                while (b1.PeekChar() != -1)
                {
                    string temp = b1.ReadString();
                    if (temp == "~")
                    {
                        string temp_name_table = b1.ReadString();
                        if (temp_name_table == name_table)
                        {
                            int time = int.Parse(b1.ReadString()) * 6;
                            for (int i = 1; i <= time; i++)
                                b1.ReadString();
                        }
                        else
                        {
                            b2.Write("~");
                            b2.Write(temp_name_table);
                        }
                        continue;
                    }
                    b2.Write(temp);
                }
                f1.Close();
                b1.Close();
                f2.Close();
                b2.Close();
                //dat
                FileStream f3 = new FileStream(path + database_name + ".dat", FileMode.Open, FileAccess.Read);
                BinaryReader b3 = new BinaryReader(f3);
                //temp.dat的文件流
                FileStream f4 = new FileStream(path + "temp.dat", FileMode.Create, FileAccess.Write);
                f4.SetLength(0);
                BinaryWriter b4 = new BinaryWriter(f4);
                while (b3.PeekChar() != -1)
                {
                    string temp = b3.ReadString();
                    if (temp == "~")
                    {
                        string temp_name_table = b3.ReadString();
                        if (temp_name_table == name_table)
                        {
                            int x = int.Parse(b3.ReadString());
                            int y = int.Parse(b3.ReadString());
                            int time = x * y;
                            for (int i = 1; i <= time; i++)
                                b3.ReadString();
                        }
                        else
                        {
                            b4.Write("~");
                            b4.Write(temp_name_table);
                        }
                        continue;
                    }
                    b4.Write(temp);
                }
                f3.Close();
                b3.Close();
                f4.Close();
                b4.Close();
                copyfile("temp.dbf", database_name + ".dbf");
                copyfile("temp.dat", database_name + ".dat");
                return 1;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return 0;
            }
        }
        public int DDL_rename()
        {
            try
            {
                string oldname = parameters[2];
                string newname = parameters[3];

                //修改dbf文件
                FileStream f1 = new FileStream(path + database_name + ".dbf", FileMode.Open, FileAccess.Read);
                BinaryReader b1 = new BinaryReader(f1);
                //temp.dbf的文件流
                FileStream f2 = new FileStream(path + "temp.dbf", FileMode.Create, FileAccess.Write);
                f2.SetLength(0);
                BinaryWriter b2 = new BinaryWriter(f2);
                while (b1.PeekChar() != -1)
                {
                    string temp = b1.ReadString();
                    if (temp == "~")
                    {
                        b2.Write(temp);
                        string temp_name_table = b1.ReadString();
                        if (temp_name_table == oldname)
                        {
                            b2.Write(newname);
                        }
                        else
                        {
                            b2.Write(temp_name_table);
                        }
                        continue;
                    }
                    b2.Write(temp);
                }
                f1.Close();
                b1.Close();
                f2.Close();
                b2.Close();
                //修改dat文件
                FileStream f3 = new FileStream(path + database_name + ".dat", FileMode.Open, FileAccess.Read);
                BinaryReader b3 = new BinaryReader(f3);
                //temp.dbf的文件流
                FileStream f4 = new FileStream(path + "temp.dat", FileMode.Create, FileAccess.Write);
                f4.SetLength(0);
                BinaryWriter b4 = new BinaryWriter(f4);
                while (b3.PeekChar() != -1)
                {
                    string temp = b3.ReadString();
                    if (temp == "~")
                    {
                        b4.Write(temp);
                        string temp_name_table = b3.ReadString();
                        if (temp_name_table == oldname)
                        {
                            b4.Write(newname);
                        }
                        else
                        {
                            b4.Write(temp_name_table);
                        }
                        continue;
                    }
                    b4.Write(temp);
                }
                f3.Close();
                b3.Close();
                f4.Close();
                b4.Close();
                copyfile("temp.dbf", database_name + ".dbf");
                copyfile("temp.dat", database_name + ".dat");
                return 1;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return 0;
            }

        }
        public int DDL_edit()
        {
            try
            {
                string name_table;
                name_table = parameters[2];

                //dbf
                FileStream fs2 = new FileStream(path + database_name + ".dbf", FileMode.Open, FileAccess.Read);
                BinaryReader br2 = new BinaryReader(fs2);
                //temp.dbf的文件流
                FileStream fswrite = new FileStream(path + "temp.dbf", FileMode.Create, FileAccess.Write);
                fswrite.SetLength(0);
                BinaryWriter bwwrite = new BinaryWriter(fswrite);
                while (br2.PeekChar() != -1)
                {
                    string temp = br2.ReadString();

                    if (temp == "~")
                    {
                        bwwrite.Write(temp);
                        string temp_name_table = br2.ReadString();
                        if (temp_name_table == name_table)
                        {
                            bwwrite.Write(temp_name_table);
                            int time = int.Parse(br2.ReadString());
                            bwwrite.Write(time.ToString());//写入 字段个数
                            for (int i = 1; i <= time * 6; i++)
                            {
                                string temp2 = br2.ReadString();
                                if (temp2 != parameters[4]) //不等于，就是没有找到name字段
                                {
                                    bwwrite.Write(temp2);
                                    continue;
                                }
                                bwwrite.Write(temp2);//写入 名字
                                int iSize = (parameters[5][0] == 'c') ? (int.Parse(parameters[5].Substring(5, 2))) : 11;
                                string type = (parameters[5][0] == 'c') ? "char" : "int";
                                int k = (parameters[6] == "key") ? 1 : 0;
                                int n = (parameters[7] == "null") ? 1 : 0;
                                int v = (parameters[8] == "valid") ? 1 : 0;
                                bwwrite.Write(type);
                                bwwrite.Write(iSize.ToString());
                                bwwrite.Write(k.ToString());
                                bwwrite.Write(n.ToString());
                                bwwrite.Write(v.ToString());
                                time--;
                            }
                        }
                        else
                        {
                            bwwrite.Write(temp_name_table);
                        }
                        continue;
                    }
                    bwwrite.Write(temp);
                }
                fs2.Close();
                br2.Close();
                fswrite.Close();
                bwwrite.Close();
                copyfile("temp.dbf", database_name + ".dbf");
                return 1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("修改表表失败!" + ex);
                return 0;
            }
        }
        public int DML_insert()
        {
            string name_table;
            name_table = parameters[2];//获取表名name_table，analyze中已经获取了数据库名字database_name
            TBL.Field[] table_insert = new TBL.Field[200];//开始读取 数据库dbf文件中的 相关结构体数据
            int ifieldnum = 0;
            FileStream fs = new FileStream(path + database_name + ".dbf", FileMode.Open, FileAccess.Read);
            BinaryReader br = new BinaryReader(fs);
            while (br.PeekChar() != -1)
            {
                if (br.ReadString() == name_table)//找到了数据库文件中的那张表
                {
                    ifieldnum = int.Parse(br.ReadString());
                    int i = 0;
                    for (i = 0; i < ifieldnum; i++)
                    {
                        table_insert[i].sFieldName = br.ReadString();
                        table_insert[i].sType = br.ReadString();
                        table_insert[i].iSize = int.Parse(br.ReadString());
                        table_insert[i].bKey = int.Parse(br.ReadString());
                        table_insert[i].bNullFlag = int.Parse(br.ReadString());
                        table_insert[i].bValidFlag = int.Parse(br.ReadString());
                    }
                }
            }
            fs.Close();
            br.Close();
            int exist_flag = 0;//表示已有数据存在
            //database_name.dat的文件流
            FileStream fs2 = new FileStream(path + database_name + ".dat", FileMode.Open, FileAccess.Read);
            BinaryReader br2 = new BinaryReader(fs2);
            //temp.dat的文件流
            //FileStream fswrite = new FileStream(@"C:\Users\顺裕\Desktop\数据库大作业\newDBMS\database\temp.dat", FileMode.Append);
            FileStream fswrite = new FileStream(path + "temp.dat", FileMode.Create, FileAccess.Write);
            fswrite.SetLength(0);
            BinaryWriter bwwrite = new BinaryWriter(fswrite);
            while (br2.PeekChar() != -1)
            {
                string temp = br2.ReadString();
                if (temp == "~")//找到了数据库文件中的那张表
                {
                    bwwrite.Write(temp);
                    string temp_name_table = br2.ReadString();
                    if (temp_name_table == name_table)
                    {
                        //在这里已经找到了这个表
                        bwwrite.Write(name_table);
                        bwwrite.Write((int.Parse(br2.ReadString()) + 1).ToString());//记录个数加1
                        bwwrite.Write(br2.ReadString());//字段数不变

                        //写入新记录
                        exist_flag = 1;
                        for (int i = 5; i <= (ifieldnum - 1) * 2 + 5; i = i + 2)
                        {
                            bwwrite.Write(parameters[i]);
                        }
                    }
                    else
                    {
                        bwwrite.Write(temp_name_table);
                    }
                    continue;
                }
                bwwrite.Write(temp);
            }
            fswrite.Close();
            bwwrite.Close();
            fs2.Close();
            br2.Close();
            //把temp.dat覆盖database_name.dat
            if (exist_flag == 1)
            {
                copyfile("temp.dat", database_name + ".dat");

            }

            //没有找到这个表
            if (exist_flag == 0)
            {
                binwrite_to_dat("~");
                binwrite_to_dat(name_table);
                binwrite_to_dat("1"); //记录数量
                binwrite_to_dat(ifieldnum.ToString()); //字段数量
                for (int i = 5; i <= (ifieldnum - 1) * 2 + 5; i = i + 2)
                {
                    binwrite_to_dat(parameters[i]);
                }
            }
            return 1;
        }
        public int copyfile(string fromwhere, string towhere)
        {
            try
            {
                FileStream f1 = new FileStream(path + fromwhere, FileMode.Open, FileAccess.Read);
                BinaryReader b1 = new BinaryReader(f1);
                FileStream f2 = new FileStream(path + towhere, FileMode.Create, FileAccess.Write);
                f2.SetLength(0);
                BinaryWriter b2 = new BinaryWriter(f2);
                while (b1.PeekChar() != -1)
                {
                    b2.Write(b1.ReadString());
                }
                f1.Close();
                b1.Close();
                f2.Close();
                b2.Close();
                return 1;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public int DDL_create()
        {
            binwrite_to_dbf("~");
            binwrite_to_dbf(parameters[2]);
            int iFieldnum = 0;//用来标记统计到了几个filed
            for (int i = 4; parameters[i] != ")"; i = i + 6)
            {

                newtable[iFieldnum].sFieldName = parameters[i];

                if (parameters[i + 1][0] == 'c')
                {
                    newtable[iFieldnum].sType = "char";
                    int end = parameters[i + 1].LastIndexOf("]");
                    string num = parameters[i + 1].Substring(5, 2);
                    newtable[iFieldnum].iSize = int.Parse(num);
                }
                else
                {
                    newtable[iFieldnum].sType = "int";
                    newtable[iFieldnum].iSize = 11;
                }

                newtable[iFieldnum].bKey = (parameters[i + 2] == "key") ? 1 : 0;
                newtable[iFieldnum].bNullFlag = (parameters[i + 3] == "null") ? 1 : 0;
                newtable[iFieldnum].bValidFlag = (parameters[i + 4] == "valid") ? 1 : 0;
                if (parameters[i + 5] == ",")
                {
                    iFieldnum++;
                }
                else
                    break;
            }
            iFieldnum++;
            binwrite_to_dbf(iFieldnum.ToString());
            for (int i = 0; i < iFieldnum; i++)
            {
                binwrite_to_dbf(newtable[i].sFieldName);
                binwrite_to_dbf(newtable[i].sType);
                binwrite_to_dbf(newtable[i].iSize.ToString());
                binwrite_to_dbf(newtable[i].bKey.ToString());
                binwrite_to_dbf(newtable[i].bNullFlag.ToString());
                binwrite_to_dbf(newtable[i].bValidFlag.ToString());
            }
            return 1;
        }
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            richTextBox2.Text = "";
            if (chaifen(richTextBox1.Text) == 1)
            {
                analyze();

                int i = 0;
                for (i = 0; parameters[i] != null; i++)
                {
                    //richTextBox2.Text += parameters[i] + "\n";
                }
            }

        }

        public void binwrite_to_dbf(string s) //写入table.dbf文件
        {
            FileStream fs = new FileStream(path + database_name + ".dbf", FileMode.Append);
            BinaryWriter bw = new BinaryWriter(fs);
            bw.Write(s);
            bw.Close();
            fs.Close();
        }
        public void binwrite_to_dat(string s) //写入table.dbf文件
        {
            FileStream fs = new FileStream(path + database_name + ".dat", FileMode.Append);
            BinaryWriter bw = new BinaryWriter(fs);
            bw.Write(s);
            bw.Close();
            fs.Close();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            //string str = "student.name";
            //string temp = str.Substring(0, str.IndexOf("."));
            //MessageBox.Show(temp);
            //temp = str.Substring(str.IndexOf(".") + 1);
            //MessageBox.Show(temp);
            //int i=searchindex("school", "student", "sno");
            //int j = searchindex("school", "student", "sname");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            treeView1.Nodes.Clear();
            //检测每个数据库中有多少个表.
            TreeNode node1 = treeView1.Nodes.Add("DATABASE");
            //DirectoryInfo dir = new DirectoryInfo("C:\\Users\\顺裕\\Desktop\\数据库大作业\\newDBMS\\database");
            DirectoryInfo dir = new DirectoryInfo(path);
            DirectoryInfo[] listdir = dir.GetDirectories();
            FileInfo[] listfile = dir.GetFiles();
            foreach (FileInfo NextFile in listfile)
            {
                if (NextFile.Name == "temp.dbf")
                { continue; }
                if (NextFile.Extension != ".dbf")
                {
                    continue;
                }
                TreeNode node2 = new TreeNode(NextFile.Name);
                node1.Nodes.Add(node2);
                //开始读取 dbf文件
                string[] name_table = new string[50];
                //richTextBox2.BackColor = Control.DefaultBackColor;
                FileStream fs = new FileStream(path + NextFile.Name, FileMode.Open, FileAccess.Read);
                BinaryReader br = new BinaryReader(fs);
                int num_table = 0;
                while (br.PeekChar() != -1)
                {

                    if (br.ReadString() == "~")
                    {
                        name_table[num_table] = br.ReadString();
                        TreeNode node3 = new TreeNode(name_table[num_table]);
                        node2.Nodes.Add(node3);
                        //richTextBox2.Text += name_table[num_table];
                    }
                }
                fs.Close();
                br.Close();

            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Form1_Load(null, null);
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            listView1.Clear();
            listView1.View = View.Details;
            listView1.Columns.Add("字段名", 60, HorizontalAlignment.Center);
            listView1.Columns.Add("类型", 60, HorizontalAlignment.Center);
            listView1.Columns.Add("长度", 60, HorizontalAlignment.Center);
            listView1.Columns.Add("KEY", 60, HorizontalAlignment.Center);
            listView1.Columns.Add("NULL", 60, HorizontalAlignment.Center);
            listView1.Columns.Add("VALID", 60, HorizontalAlignment.Center);
            //首先获取 数据库名，和表名
            if (treeView1.SelectedNode.Text == "DATABASE" || treeView1.SelectedNode.Text.Contains(".dbf"))
                return;
            string name_table = treeView1.SelectedNode.Text;
            string str = treeView1.SelectedNode.Parent.Text;
            string name_database = str.Substring(0, str.IndexOf('.'));
            int num_ziduan = 0;//字段的数量

            int num_jilu = 0;//记录的数量
            //for (int i = 0; i < 10; i++)
            //{
            //    ListViewItem lvi = new ListViewItem();
            //    lvi.Text = "1列";
            //    lvi.SubItems.Add("2列第" + i);
            //    lvi.SubItems.Add("3列第" + i);
            //    this.listView1.Items.Add(lvi);
            //}
            //MessageBox.Show(name_database + "  " + name_table);
            FileStream f1 = new FileStream(path + name_database + ".dbf", FileMode.Open, FileAccess.Read);
            BinaryReader b1 = new BinaryReader(f1);
            while (b1.PeekChar() != -1)
            {
                if (b1.ReadString() == "~")
                {
                    if (b1.ReadString() == name_table)
                    {
                        num_ziduan = int.Parse(b1.ReadString());
                        for (int i = 0; i < num_ziduan; i++)
                        {
                            ListViewItem lvi = new ListViewItem();
                            lvi.Text = b1.ReadString();
                            newtable[i].sFieldName = lvi.Text;
                            lvi.SubItems.Add(b1.ReadString());
                            lvi.SubItems.Add(b1.ReadString());
                            lvi.SubItems.Add(b1.ReadString());
                            lvi.SubItems.Add(b1.ReadString());
                            lvi.SubItems.Add(b1.ReadString());
                            this.listView1.Items.Add(lvi);
                        }
                        break;
                    }
                }
            }

            f1.Close();
            b1.Close();
            //构建listview2
            listView2.Clear();
            listView2.View = View.Details;
            for (int i = 0; i < num_ziduan; i++)
            {
                listView2.Columns.Add(newtable[i].sFieldName, 60, HorizontalAlignment.Center);
            }
            FileStream f2 = new FileStream(path + name_database + ".dat", FileMode.Open, FileAccess.Read);
            BinaryReader b2 = new BinaryReader(f2);
            while (b2.PeekChar() != -1)
            {
                if (b2.ReadString() == "~")
                {
                    if (b2.ReadString() == name_table)
                    {
                        num_jilu = int.Parse(b2.ReadString());
                        b2.ReadString();//跳过一个值，这个值就是num_ziduan,只不过已经 存过了
                        for (int i = 0; i < num_jilu; i++)
                        {
                            ListViewItem lvi = new ListViewItem();
                            lvi.Text = b2.ReadString();
                            for (int j = 0; j < num_ziduan - 1; j++)
                            {
                                lvi.SubItems.Add(b2.ReadString());
                            }
                            this.listView2.Items.Add(lvi);
                        }
                        break;
                    }
                }
            }
            f2.Close();
            b2.Close();
        }
    }
}
