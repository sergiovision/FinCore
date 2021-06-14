<chart>
id=132091339185315885
symbol=GBPUSD
period_type=1
period_size=24
digits=5
tick_size=0.000000
position_time=1563494400
scale_fix=0
scale_fixed_min=1.191300
scale_fixed_max=1.280800
scale_fix11=0
scale_bar=0
scale_bar_val=1.000000
scale=16
mode=0
fore=0
grid=0
volume=0
scroll=0
shift=1
shift_size=19.825708
fixed_pos=0.000000
ohlc=0
one_click=0
one_click_btn=1
bidline=1
askline=1
lastline=0
days=1
descriptions=0
tradelines=1
window_left=1077
window_top=0
window_right=1436
window_bottom=126
window_type=1
floating=0
floating_left=0
floating_top=0
floating_right=0
floating_bottom=0
floating_type=1
floating_toolbar=1
floating_tbstate=
background_color=0
foreground_color=16777215
barup_color=32768
bardown_color=255
bullcandle_color=16777215
bearcandle_color=16777215
chartline_color=16777215
volumes_color=32768
grid_color=12632256
bidline_color=12632256
askline_color=12632256
lastline_color=12632256
stops_color=17919
windows_total=1

<window>
height=121.141818
objects=0

<indicator>
name=Main
path=
apply=1
show_data=1
scale_inherit=0
scale_line=0
scale_line_percent=50
scale_line_value=0.000000
scale_fix_min=0
scale_fix_min_val=0.000000
scale_fix_max=0
scale_fix_max_val=0.000000
expertmode=0
fixed_height=-1
</indicator>

<indicator>
name=Custom Indicator
path=Indicators\BBands
apply=0
show_data=1
scale_inherit=0
scale_line=0
scale_line_percent=50
scale_line_value=0.000000
scale_fix_min=0
scale_fix_min_val=0.000000
scale_fix_max=0
scale_fix_max_val=0.000000
expertmode=536871940
fixed_height=-1

<graph>
name=Upper Bollinger
draw=1
style=4
width=1
color=12632256
</graph>

<graph>
name=Middle Bollinger
draw=1
style=4
width=1
color=12632256
</graph>

<graph>
name=Lower Bollinger
draw=1
style=4
width=1
color=12632256
</graph>
<inputs>
BandsPeriod=20
BandsDeviation=1.6
MA_Method=0
IPC=1
</inputs>
</indicator>

<indicator>
name=Custom Indicator
path=Indicators\Market\SSA Fast Trend Forecast.ex5
apply=0
show_data=1
scale_inherit=0
scale_line=0
scale_line_percent=50
scale_line_value=0.000000
scale_fix_min=0
scale_fix_min_val=0.000000
scale_fix_max=0
scale_fix_max_val=0.000000
expertmode=4
fixed_height=-1

<graph>
name=CAT(1024, N/4, 0.25)  // 
draw=10
style=0
width=2
arrow=251
shift=12
color=16711680,13749760
</graph>
<inputs>
ForecastMethod=2
SegmentLength=1024
SW=6
FastNoiseLevel=0.25
SlowNoiseLevel=0.25
FrcstConvertMethod=1
ForecastSmoothON=0
NWindVolat=16
RefreshPeriod=60
ForecastPoints=12
BackwardShift=0
_VISUAL_OPTIONS_=* VISUAL OPTIONS *
NormalColor=16711680
PredictColor=13749760
_INTERFACE_=* INTERFACE *
magic_numb=19661021100
WriteFileON=0
InpNumRecords=0
</inputs>
</indicator>

<indicator>
name=Custom Indicator
path=Indicators\FinCoreLevels.ex5
apply=1
show_data=1
scale_inherit=0
scale_line=0
scale_line_percent=50
scale_line_value=0.000000
scale_fix_min=0
scale_fix_min_val=0.000000
scale_fix_max=0
scale_fix_max_val=0.000000
expertmode=4
fixed_height=-1
</window>
</chart>
<inputs>
levels_string=
tcp_host=127.0.0.1
tcp_port=2022
</inputs>
</indicator>

<indicator>
name=Custom Indicator
path=Indicators\LevelsHistogram.ex5
apply=0
show_data=1
scale_inherit=0
scale_line=0
scale_line_percent=50
scale_line_value=0.000000
scale_fix_min=0
scale_fix_min_val=0.000000
scale_fix_max=0
scale_fix_max_val=0.000000
expertmode=4
fixed_height=-1

<graph>
name=
draw=0
style=0
width=1
color=
</graph>
<inputs>
DayTheHistogram=10
DaysForCalculation=365
RangePercent=70
Standalone=true
InnerRange=8519755
OuterRange=16711935
ControlPoint=42495
ShowValue=true
</inputs>
</indicator>

<indicator>
name=Custom Indicator
path=Indicators\AverageVolumes.ex5
apply=0
show_data=1
scale_inherit=0
scale_line=0
scale_line_percent=50
scale_line_value=0.000000
scale_fix_min=1
scale_fix_min_val=0.000000
scale_fix_max=0
scale_fix_max_val=0.000000
expertmode=4
fixed_height=-1

<graph>
name=
draw=1
style=0
width=1
arrow=251
shift=48
color=11186720
</graph>

<graph>
name=4 week average
draw=11
style=0
width=6
arrow=251
color=32768,255
</graph>
<inputs>
inpNumAver=4
inpVolMod=0
</inputs>
</indicator>


</window>
</chart>