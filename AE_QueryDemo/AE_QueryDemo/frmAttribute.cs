using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AE_QueryDemo
{
    public partial class frmAttribute : Form
    {
        IMap pMap = Global.pOriginalMap;
        IWorkspace pWorkspace = Global.pWorkspace1;


        public frmAttribute()
        {
            InitializeComponent();
        }

        private void Attribute_Load(object sender, EventArgs e)
        {
            lbShow.Items.Clear();
            for (int i = 0; i < pMap.LayerCount; i++)
            {
                //如果是栅格数据就跳过本次循环
                if(pMap.get_Layer(i) is IRasterLayer)
                {
                    continue;
                }
                cboSelectLayer.SelectedIndex = 0;
                AddFields();
            }
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            //将工作空间强转成要素工作空间
            IFeatureWorkspace pFtWorkspace = pWorkspace as IFeatureWorkspace;
            //通过要素工作空间打开cboSelectLayer选择的图层, 并放在要素类中
            IFeatureClass pFtClass = pFtWorkspace.OpenFeatureClass(cboSelectLayer.Text);
            try
            {
                //恶心查询方法, 返回类型为DataTable
                Global.myDGV1.DataSource = Search(pFtClass, txtSql.Text.Trim());
            }
            catch (Exception ee)//这里不能用e变量名？
            {
                MessageBox.Show(ee.Message);
            }

        }
        /// <summary>
        /// 核心查询函数
        /// </summary>
        /// <param name="pFtClass"></param>
        /// <param name="pWhereClause"></param>
        /// <returns></returns>
        private static DataTable Search(IFeatureClass pFtClass, string pWhereClause)
        {
            //定义过滤器对象
            IQueryFilter pQueryFilter = new QueryFilter();
            //设置sql查询语句
            pQueryFilter.WhereClause = pWhereClause;
            //设置游标
            //调用.Search方法;false表示游标到达最后一条要素以后不回收
            //IFeatureCursor Search(IQueryFilter filter, bool Recycling);
            IFeatureCursor pFtCursor = pFtClass.Search(pQueryFilter, false);
            //声明一个pFt要素并将查询结果中的第一条Feature赋值给它
            IFeature pFt = pFtCursor.NextFeature();
            //实例化一个DataTable内存表对象, 即函数最后返回的DataTable对象
            DataTable DT = new DataTable();
            for (int i = 0; i < pFtCursor.Fields.FieldCount; i++)
            {
                //首先设置DT字段
                DataColumn dc = new DataColumn(pFtCursor.Fields.get_Field(i).Name,
                    System.Type.GetType(ParseFieldType(pFtCursor.Fields.get_Field(i).Type)));
                //字段生成完成后添加到DT的列中
                DT.Columns.Add(dc);
            }
            while (pFt != null)
            {
                //向DT装导入字段值
                DataRow dr = DT.NewRow();
                //以DT的表结构新建行对象
                for (int i = 0; i < pFt.Fields.FieldCount; i++)
                {
                    dr[i] = pFt.get_Value(i);
                }
                //完成某一行的字段值录入后向DT中添加此行对象
                DT.Rows.Add(dr);
                //转到下一个要素
                pFt = pFtCursor.NextFeature();
            }
            return DT;//返回DataTable对象
        }
        /// <summary>
        /// 转换数据类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static string ParseFieldType(esriFieldType FieldType)
        {
            switch (FieldType)
            {
                case esriFieldType.esriFieldTypeInteger:
                    return "System.Int32";
                case esriFieldType.esriFieldTypeOID:
                    return "System.Int32";
                case esriFieldType.esriFieldTypeDouble:
                    return "System.Double";
                case esriFieldType.esriFieldTypeDate:
                    return "System.DateTime";
                default:
                    return "System.String";
            }
        }

        private void cboSelectLayer_SelectedIndexChanged(object sender, EventArgs e)
        {
            AddFields();
        }

        /// <summary>
        /// 向ListBox中添加图层字段
        /// </summary>
        private void AddFields()
        {
            //清空ListBox
            lbShow.Items.Clear();
            //将pWorkspace强转成要素工作空间
            IFeatureWorkspace pFtWorkspace = pWorkspace as IFeatureWorkspace;
            //通过要素工作空间打开cboSelectLayer选择的图层, 并放在要素类中
            IFeatureClass pFtClass = pFtWorkspace.OpenFeatureClass(cboSelectLayer.Text);
            //打开游标, null为查询全部, false表示游标到达最后一条要素以后不回收
            IFeatureCursor pFtCursor = pFtClass.Search(null, false);
            //使用for循环逐一添加图层字段
            for (int i = 0; i < pFtCursor.Fields.FieldCount; i++)
            {
                IField pField = pFtCursor.Fields.get_Field(i); //获取字段的下标
                cboSelectLayer.Items.Add(pField.Name.ToString());//将字段名加到ComboBox中
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            txtSql.Text += " = ";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            txtSql.Text += " ! ";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            txtSql.Text += " like ";
        }

        private void button4_Click(object sender, EventArgs e)
        {
            txtSql.Text += " > ";
        }

        private void button5_Click(object sender, EventArgs e)
        {
            txtSql.Text += " >= ";
        }

        private void button6_Click(object sender, EventArgs e)
        {
            txtSql.Text += " and ";
        }

        private void button7_Click(object sender, EventArgs e)
        {
            txtSql.Text += " < ";
        }

        private void button8_Click(object sender, EventArgs e)
        {
            txtSql.Text += " <= ";
        }

        private void button9_Click(object sender, EventArgs e)
        {
            txtSql.Text += " or ";
        }

        private void button10_Click(object sender, EventArgs e)
        {
            txtSql.Text += " ? ";
        }

        private void button11_Click(object sender, EventArgs e)
        {
            txtSql.Text += " () ";
        }

        private void button12_Click(object sender, EventArgs e)
        {
            txtSql.Text += " not ";
        }

        private void button13_Click(object sender, EventArgs e)
        {
            txtSql.Text += " * ";
        }

        private void button14_Click(object sender, EventArgs e)
        {
            txtSql.Text += " is ";
        }

        private void lbValue_DoubleClick(object sender, EventArgs e)
        {
            //使用object引用对象进行装箱
            object var = lbValue.SelectedItem;
            //使用问号表达式来进行判断
            txtSql.Text += var.GetType().FullName == "System.String" ? "'" + var + "'" : var.ToString();
        }

        private void btnGetValue_Click(object sender, EventArgs e)
        {
            lbValue.Items.Clear();
            //将pWorkspace强转成要素工作空间
            IFeatureWorkspace pFtWorkspace = pWorkspace as IFeatureWorkspace;
            //通过要素工作空间打开cboSelectLayer选择的图层, 并放在要素类中
            IFeatureClass pFtClass = pFtWorkspace.OpenFeatureClass(cboSelectLayer.Text);
            //打开游标, null为查询全部, false表示游标到达最后一条要素以后不回收
            IFeatureCursor pFtCursor = pFtClass.Search(null, false);
            //声明一个pFt要素并将查询结果中的第一条Feature赋值给它
            IFeature pFt = pFtCursor.NextFeature();
            //声明index作为下标使用
            int index = 0;
            //将index赋值为找到选中字段下标
            index = pFtCursor.FindField(lbShow.Text);
            //当要素不为空时
            while(pFt!=null)
            {
                //如果lbValue中包含要素下标则游标到下一个要素, 跳过本次循环
                if(lbValue.Items.Contains(pFt.get_Value(index)))
                {
                    pFt = pFtCursor.NextFeature();
                    continue;
                }
                //将游标的的要素加到lbValue中
                lbValue.Items.Add(pFt.get_Value(index));
                //转到下一个要素
                pFt = pFtCursor.NextFeature();
            }

        }

        private void lbShow_DoubleClick(object sender, EventArgs e)
        {
            object var = lbShow.SelectedItem;
            txtSql.Text += var.GetType().FullName == "System.String" ? "'" + var + "'"
           : var.ToString();
        }
    }
}
