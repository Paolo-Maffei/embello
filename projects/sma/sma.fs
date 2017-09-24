\ access HC05 Bluetooth module via UART2, for use with SMA inverter - part 1/2
\ -jcw, 2019-09-19

compiletoram? [if]  forgetram  [then]

PA0 constant BT-KEY
PA1 constant BT-RESET

[ifndef] uart-irq-init
include ../../explore/1608-forth/flib/any/ring.fs
include ../../explore/1608-forth/flib/stm32f1/uart2.fs
include ../../explore/1608-forth/flib/stm32f1/uart2-irq.fs
[then]

include config.fs

: uart-s. ( addr len -- )
  0 ?do
    dup c@ [char] % = if
      myAddr-str recurse  \ expand % by sending myAddr-str
    else
      dup c@ uart-emit
    then
    1+
  loop drop ;

: uart-cr ( -- ) $0D uart-emit  $0A uart-emit ;

: at-cmd ( addr len -- )
  s" AT" uart-s.  uart-s.  uart-cr ;

: listen
  500 ms
  begin
    uart-irq-key? while
    uart-irq-key emit
  repeat ;

: listen-hex
  500 ms
  begin
    uart-irq-key? while
    uart-irq-key h.2 space
  repeat ;

: long-listen
  5 0 do listen loop ;

: at-show ( addr len -- )
  at-cmd listen ;

: bt-init ( f -- )
  uart-irq-init
  38400 uart-baud
  OMODE-PP BT-KEY   io-mode!  BT-KEY io!
  OMODE-PP BT-RESET io-mode!  BT-RESET ioc! 10 ms  BT-RESET ios! 2000 ms ;

: bt-pair
  true bt-init
  10 . s" "                at-show
  11 . s" +VERSION?"       at-show
  12 . s" +ORGL"           at-show
  13 . s" +NAME=SMA_SOLAR" at-show
  14 . s" +PSWD=0000"      at-show
  15 . s" +ROLE=1"         at-show
  16 . s" +RMAAD"          at-show
  17 . s" +UART=38400,0,0" at-show
  18 . s" +CMODE=1"        at-show
  19 . s" +INIT"           at-show
  20 . s" +STATE?"         at-show
  21 . s" +IAC=9e8b33"     at-show
  22 . s" +CLASS=1F04"     at-show
  23 . s" +INQM=1,1,10"    at-show
  24 . s" +INQ"            at-show long-listen
  25 . s" +RNAME?%"        at-show long-listen
  26 . s" +PAIR=%,20"      at-show long-listen
  27 . s" +FSAD=%"         at-show
  28 . s" +ADDR?"          at-show long-listen
  29 . s" +MRAD?"          at-show long-listen
  30 . s" +BIND=%"         at-show
  31 . s" +CMODE=0"        at-show
  32 . s" +STATE?"         at-show
\ 33 . s" +RESET"          at-show
  BT-RESET ioc! ;

\ the following was ported from this just-as-incomprehensible C code in JeeLib:
\   https://github.com/jcw/jeelib/blob/master/examples/RF12/smaRelay/smaComms.h
\ that was hacked up and worked for 5 years in a row, so who cares what it does

create fcstab
hex
  $0000 h, $1189 h, $2312 h, $329B h, $4624 h, $57AD h, $6536 h, $74BF h,
  $8C48 h, $9DC1 h, $AF5A h, $BED3 h, $CA6C h, $DBE5 h, $E97E h, $F8F7 h,
  $1081 h, $0108 h, $3393 h, $221A h, $56A5 h, $472C h, $75B7 h, $643E h,
  $9CC9 h, $8D40 h, $BFDB h, $AE52 h, $DAED h, $CB64 h, $F9FF h, $E876 h,
  $2102 h, $308B h, $0210 h, $1399 h, $6726 h, $76AF h, $4434 h, $55BD h,
  $AD4A h, $BCC3 h, $8E58 h, $9FD1 h, $EB6E h, $FAE7 h, $C87C h, $D9F5 h,
  $3183 h, $200A h, $1291 h, $0318 h, $77A7 h, $662E h, $54B5 h, $453C h,
  $BDCB h, $AC42 h, $9ED9 h, $8F50 h, $FBEF h, $EA66 h, $D8FD h, $C974 h,
  $4204 h, $538D h, $6116 h, $709F h, $0420 h, $15A9 h, $2732 h, $36BB h,
  $CE4C h, $DFC5 h, $ED5E h, $FCD7 h, $8868 h, $99E1 h, $AB7A h, $BAF3 h,
  $5285 h, $430C h, $7197 h, $601E h, $14A1 h, $0528 h, $37B3 h, $263A h,
  $DECD h, $CF44 h, $FDDF h, $EC56 h, $98E9 h, $8960 h, $BBFB h, $AA72 h,
  $6306 h, $728F h, $4014 h, $519D h, $2522 h, $34AB h, $0630 h, $17B9 h,
  $EF4E h, $FEC7 h, $CC5C h, $DDD5 h, $A96A h, $B8E3 h, $8A78 h, $9BF1 h,
  $7387 h, $620E h, $5095 h, $411C h, $35A3 h, $242A h, $16B1 h, $0738 h,
  $FFCF h, $EE46 h, $DCDD h, $CD54 h, $B9EB h, $A862 h, $9AF9 h, $8B70 h,
  $8408 h, $9581 h, $A71A h, $B693 h, $C22C h, $D3A5 h, $E13E h, $F0B7 h,
  $0840 h, $19C9 h, $2B52 h, $3ADB h, $4E64 h, $5FED h, $6D76 h, $7CFF h,
  $9489 h, $8500 h, $B79B h, $A612 h, $D2AD h, $C324 h, $F1BF h, $E036 h,
  $18C1 h, $0948 h, $3BD3 h, $2A5A h, $5EE5 h, $4F6C h, $7DF7 h, $6C7E h,
  $A50A h, $B483 h, $8618 h, $9791 h, $E32E h, $F2A7 h, $C03C h, $D1B5 h,
  $2942 h, $38CB h, $0A50 h, $1BD9 h, $6F66 h, $7EEF h, $4C74 h, $5DFD h,
  $B58B h, $A402 h, $9699 h, $8710 h, $F3AF h, $E226 h, $D0BD h, $C134 h,
  $39C3 h, $284A h, $1AD1 h, $0B58 h, $7FE7 h, $6E6E h, $5CF5 h, $4D7C h,
  $C60C h, $D785 h, $E51E h, $F497 h, $8028 h, $91A1 h, $A33A h, $B2B3 h,
  $4A44 h, $5BCD h, $6956 h, $78DF h, $0C60 h, $1DE9 h, $2F72 h, $3EFB h,
  $D68D h, $C704 h, $F59F h, $E416 h, $90A9 h, $8120 h, $B3BB h, $A232 h,
  $5AC5 h, $4B4C h, $79D7 h, $685E h, $1CE1 h, $0D68 h, $3FF3 h, $2E7A h,
  $E70E h, $F687 h, $C41C h, $D595 h, $A12A h, $B0A3 h, $8238 h, $93B1 h,
  $6B46 h, $7ACF h, $4854 h, $59DD h, $2D62 h, $3CEB h, $0E70 h, $1FF9 h,
  $F78F h, $E606 h, $D49D h, $C514 h, $B1AB h, $A022 h, $92B9 h, $8330 h,
  $7BC7 h, $6A4E h, $58D5 h, $495C h, $3DE3 h, $2C6A h, $1EF1 h, $0F78 h,
decimal

\ continued in sma2.fs ...
