<chart>
id=132996141196366262
symbol=GBPAUD.
description=Great Britain Pound vs Australian Dollar
period_type=1
period_size=24
digits=5
tick_size=0.000000
position_time=1655154000
scale_fix=0
scale_fixed_min=1.677800
scale_fixed_max=1.792000
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
shift_size=19.337017
fixed_pos=0.000000
ticker=1
ohlc=0
one_click=0
one_click_btn=1
bidline=1
askline=1
lastline=0
days=1
descriptions=0
tradelines=1
tradehistory=1
window_left=182
window_top=182
window_right=1434
window_bottom=576
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
objects=20776

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


</window>
</chart>
