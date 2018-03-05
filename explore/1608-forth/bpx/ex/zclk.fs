\ generate a clock signal for Z80 use

forgetram

PB6 constant ZCLK

: zclk-10Hz ( -- )  \ slow clock for debugging, 50% duty, 4 ns rise/fall
  10 ZCLK pwm-init
  5000 ZCLK pwm ;

: zclk-4MHz ( -- )  \ approx 123 ns high, 127 ns low, 49% duty
  7200 ZCLK pwm-init   \ first set up pwm correctly
  17 4 timer-init      \ then mess with the timer divider, i.e. ÷18
  9991 ZCLK pwm ;      \ finally, set the pwm to still toggle at approx 50%

: zclk-8MHz ( -- )  \ approx 54 ns high, 71 ns low, 43% duty
  7200 ZCLK pwm-init   \ first set up pwm correctly
   8 4 timer-init      \ then mess with the timer divider, i.e. ÷9
  9995 ZCLK pwm ;      \ finally, set the pwm to still toggle at approx 50%

: zclk-18MHz ( -- )  \ approx 25 ns high, 30 ns low, 46% duty
  7200 ZCLK pwm-init   \ first set up pwm correctly
   3 4 timer-init      \ then mess with the timer divider, i.e. ÷4
  9998 ZCLK pwm ;      \ finally, set the pwm to still toggle at approx 50%

\ this one is way out of spec for the Z84C0020, but could be used for the eZ80
: zclk-36MHz ( -- )  \ approx 13 ns high, 15 ns low, 45% duty
  7200 ZCLK pwm-init   \ first set up pwm correctly
   1 4 timer-init      \ then mess with the timer divider, i.e. ÷2
  9999 ZCLK pwm ;      \ finally, set the pwm to still toggle at approx 50%

zclk-10Hz
