#property library
#property copyright "Sergei Zhuravlev"
#property link      "http://github.com/sergiovision"
#property strict

#include <ChartObjects\ChartObjectsTxtControls.mqh>

class CProgressBar :public CObject
{
private:
   color                pb_backcolor, pb_forecolor, pb_textcolor;
   ENUM_BASE_CORNER     pb_corner;
   ENUM_ANCHOR_POINT    pb_anchor;
   string               pb_font, pb_title;
   int                  pb_fontsize;
   int                  pb_percent;
   CChartObjectLabel    *pb_text,*pb_fore, *pb_back;
public:
   int                  Minimum;
   int                  Maximum;
   
                        CProgressBar();
                        ~CProgressBar(){Delete();};
   bool                 Init();
   void                 Delete();
   bool                 Create(long chart_id,string name,int window,int X,int Y);
   bool                 Value(int value);
   ENUM_BASE_CORNER     Corner(){return(pb_corner);}
   void                 Corner(ENUM_BASE_CORNER corner){pb_corner=corner;Redraw();}
   ENUM_ANCHOR_POINT    Anchor(){return(pb_anchor);}
   void                 Anchor(ENUM_ANCHOR_POINT anchor){pb_anchor=anchor;Redraw();}
   void                 Text(string title){pb_title=title;Redraw();}
   bool                 Redraw();
};
CProgressBar::CProgressBar()
{
   pb_text=NULL;
   pb_fore=NULL;
   pb_back=NULL;
   pb_corner=CORNER_LEFT_LOWER;
   pb_anchor=ANCHOR_CENTER;
   pb_font="Tahoma";
   pb_title="";
   pb_fontsize=10;
   pb_backcolor=Silver;
   pb_forecolor=DodgerBlue;
   pb_textcolor=Black;
   pb_percent=-1;
   Minimum=0;
   Maximum=100;
}
bool CProgressBar::Create(long chart_id,string name,int window,int X,int Y)
{
   bool result;
   pb_back=new CChartObjectLabel;
   result=pb_back.Create(chart_id,"0"+name,window,X,Y);
   
   pb_fore=new CChartObjectLabel;
   result&=pb_fore.Create(chart_id,"1"+name,window,X,Y);

   pb_text=new CChartObjectLabel;
   result&=pb_text.Create(chart_id,name+"text",window,X,Y);
   result&=pb_fore.Description("");
   result&=pb_text.Description("");
   result&=Redraw();
   return(result);
}
bool CProgressBar::Redraw(void)
{
   bool result;
   if(pb_back==NULL || pb_fore==NULL || pb_text==NULL) return(false);

   result=pb_back.Corner(pb_corner);
   result&=pb_back.Anchor(pb_anchor);
   result&=pb_back.Font("Webdings");
   result&=pb_back.FontSize(pb_fontsize+1);
   result&=pb_back.Color(pb_backcolor);
   result&=pb_back.Description("gggggggggggggggggg");

   result&=pb_fore.Corner(pb_corner);
   result&=pb_fore.Anchor(pb_anchor);
   result&=pb_fore.Font("Webdings");
   result&=pb_fore.FontSize(pb_fontsize);
   result&=pb_fore.Color(pb_forecolor);

   result&=pb_text.Corner(pb_corner);
   result&=pb_text.Anchor(pb_anchor);
   result&=pb_text.Font(pb_font);
   result&=pb_text.FontSize(pb_fontsize);
   result&=pb_text.Color(pb_textcolor);
   return(result);
}
bool CProgressBar::Value(int value)
{
   string str="";
   int mod;
   bool  result = 0;
   int percent= int(NormalizeDouble(value/((Maximum-Minimum)/100.0),0));
   if(pb_percent==percent) return (true);
   pb_percent=percent;
   result&=pb_text.Description(pb_title+" "+IntegerToString(percent)+" %");
   mod=percent/5;
   for(int i=0; i<=percent/5-1;i++)
      str=str+"g";
   result&=pb_fore.Description(str);
   ChartRedraw(pb_text.ChartId());
   return(result);
}
void CProgressBar::Delete()
{
   delete pb_text;
   delete pb_fore;
   delete pb_back;
}