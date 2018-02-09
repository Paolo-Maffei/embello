\ Drive a TM1638 chip for 7-segment output and key matrix input
\ bit-banged implementation: at 72 MHz, send takes 200 µs and receive 32 µs

[ifndef] TM.STB  PA4 constant TM.STB  [then]
[ifndef] TM.CLK  PA5 constant TM.CLK  [then]
[ifndef] TM.DIO  PA7 constant TM.DIO  [then]

\ 8-byte display buffer, shown left-to-right, b0 = seg A, b7 = dec point
$11223344 $55667788 2variable 7seg-data

: tm1638-out ( u -- )  \ shift out 8 bits to the display controller
  8 0 do
    dup 1 and TM.DIO io!
    shr
    TM.CLK ios!
    nop  \ slow down a bit
    TM.CLK ioc!
  loop drop ;

: tm1638-init ( -- )  \ initialise the I/O pins for output
  TM.STB ios!
  OMODE-OD TM.STB io-mode!
  OMODE-OD TM.CLK io-mode!
  OMODE-OD TM.DIO io-mode! ;

: tm1638-send ( -- )  \ send contents of display buffer to controller
  TM.STB ioc!
  $88 tm1638-out
  TM.STB ios!
  nop
  TM.STB ioc!
  $C0 tm1638-out
  8 0 do
      0  \ awful bit-reshuffle needed, to flip row- and column-wise order
      8 0 do
        shl  i 7seg-data + c@  j rshift  1 and or
      loop
      tm1638-out  0 tm1638-out
  loop
  TM.STB ios! ;

: tm1638-recv ( -- u )  \ receive key matrix status from controller
  TM.STB ioc!
  $42 tm1638-out
  TM.DIO ios!  \ open-drain, so controller can reply
  0
  32 0 do
    TM.CLK ios!
    nop  \ slow down a bit
    TM.CLK ioc!
    shl  TM.DIO io@  1 and or
  loop
  TM.STB ios! ;

: tm1638-listen
  begin
    tm1638-recv ?dup if cr hex. then
    50 ms
  key? until ;

\ tm1638-init
\ tm1638-send
\ tm1638-listen
