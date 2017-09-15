\ access HC05 Bluetooth module via UART2, for use with SMA solar inverter

compiletoram? [if]  forgetram  [then]

PA0 constant BT-KEY
PA1 constant BT-RESET

[ifndef] uart2-init
include ../../flib/any/ring.fs
include ../../flib/stm32f1/uart2.fs
include ../../flib/stm32f1/uart2-irq.fs
[then]

: uart-s. ( addr len -- )
  0 ?do dup c@ uart-emit 1+ loop drop ;

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
  OMODE-PP BT-RESET io-mode!  BT-RESET ioc! 10 ms
                              BT-RESET ios! 2000 ms ;

: try
  true bt-init
  10 . s" "                      at-show
  11 . s" +VERSION?"             at-show
  12 . s" +ORGL"                 at-show
  13 . s" +NAME=SMA_SOLAR"       at-show
  14 . s" +PSWD=0000"            at-show
  15 . s" +ROLE=1"               at-show
  16 . s" +RMAAD"                at-show
  17 . s" +UART=38400,0,0"       at-show
  18 . s" +CMODE=1"              at-show
  19 . s" +INIT"                 at-show
  20 . s" +STATE?"               at-show
  21 . s" +IAC=9e8b33"           at-show
  22 . s" +CLASS=1F04"           at-show
  23 . s" +INQM=1,1,10"          at-show
  24 . s" +INQ"                  at-show long-listen
  25 . s" +RNAME?80,25,A4EC14"   at-show long-listen
  26 . s" +PAIR=80,25,A4EC14,20" at-show
  27 . s" +FSAD=80,25,A4EC14"    at-show
  28 . s" +ADDR?"                at-show long-listen
  29 . s" +MRAD?"                at-show long-listen
  30 . s" +BIND=80,25,A4EC14"    at-show
  31 . s" +CMODE=0"              at-show
  32 . s" +STATE?"               at-show
\ 33 . s" +RESET"                at-show
  BT-RESET ioc! ;

\ the following was ported from this just-as-incomprehensible C code in JeeLib:
\   https://github.com/jcw/jeelib/blob/master/examples/RF12/smaRelay/smaComms.h
\ that was hacked up and worked for 5 years in a row, so who cares what it does

create fcstab
hex
  $0000 h, $1189 h, $2312 h, $329b h, $4624 h, $57ad h, $6536 h, $74bf h,
  $8c48 h, $9dc1 h, $af5a h, $bed3 h, $ca6c h, $dbe5 h, $e97e h, $f8f7 h,
  $1081 h, $0108 h, $3393 h, $221a h, $56a5 h, $472c h, $75b7 h, $643e h,
  $9cc9 h, $8d40 h, $bfdb h, $ae52 h, $daed h, $cb64 h, $f9ff h, $e876 h,
  $2102 h, $308b h, $0210 h, $1399 h, $6726 h, $76af h, $4434 h, $55bd h,
  $ad4a h, $bcc3 h, $8e58 h, $9fd1 h, $eb6e h, $fae7 h, $c87c h, $d9f5 h,
  $3183 h, $200a h, $1291 h, $0318 h, $77a7 h, $662e h, $54b5 h, $453c h,
  $bdcb h, $ac42 h, $9ed9 h, $8f50 h, $fbef h, $ea66 h, $d8fd h, $c974 h,
  $4204 h, $538d h, $6116 h, $709f h, $0420 h, $15a9 h, $2732 h, $36bb h,
  $ce4c h, $dfc5 h, $ed5e h, $fcd7 h, $8868 h, $99e1 h, $ab7a h, $baf3 h,
  $5285 h, $430c h, $7197 h, $601e h, $14a1 h, $0528 h, $37b3 h, $263a h,
  $decd h, $cf44 h, $fddf h, $ec56 h, $98e9 h, $8960 h, $bbfb h, $aa72 h,
  $6306 h, $728f h, $4014 h, $519d h, $2522 h, $34ab h, $0630 h, $17b9 h,
  $ef4e h, $fec7 h, $cc5c h, $ddd5 h, $a96a h, $b8e3 h, $8a78 h, $9bf1 h,
  $7387 h, $620e h, $5095 h, $411c h, $35a3 h, $242a h, $16b1 h, $0738 h,
  $ffcf h, $ee46 h, $dcdd h, $cd54 h, $b9eb h, $a862 h, $9af9 h, $8b70 h,
  $8408 h, $9581 h, $a71a h, $b693 h, $c22c h, $d3a5 h, $e13e h, $f0b7 h,
  $0840 h, $19c9 h, $2b52 h, $3adb h, $4e64 h, $5fed h, $6d76 h, $7cff h,
  $9489 h, $8500 h, $b79b h, $a612 h, $d2ad h, $c324 h, $f1bf h, $e036 h,
  $18c1 h, $0948 h, $3bd3 h, $2a5a h, $5ee5 h, $4f6c h, $7df7 h, $6c7e h,
  $a50a h, $b483 h, $8618 h, $9791 h, $e32e h, $f2a7 h, $c03c h, $d1b5 h,
  $2942 h, $38cb h, $0a50 h, $1bd9 h, $6f66 h, $7eef h, $4c74 h, $5dfd h,
  $b58b h, $a402 h, $9699 h, $8710 h, $f3af h, $e226 h, $d0bd h, $c134 h,
  $39c3 h, $284a h, $1ad1 h, $0b58 h, $7fe7 h, $6e6e h, $5cf5 h, $4d7c h,
  $c60c h, $d785 h, $e51e h, $f497 h, $8028 h, $91a1 h, $a33a h, $b2b3 h,
  $4a44 h, $5bcd h, $6956 h, $78df h, $0c60 h, $1de9 h, $2f72 h, $3efb h,
  $d68d h, $c704 h, $f59f h, $e416 h, $90a9 h, $8120 h, $b3bb h, $a232 h,
  $5ac5 h, $4b4c h, $79d7 h, $685e h, $1ce1 h, $0d68 h, $3ff3 h, $2e7a h,
  $e70e h, $f687 h, $c41c h, $d595 h, $a12a h, $b0a3 h, $8238 h, $93b1 h,
  $6b46 h, $7acf h, $4854 h, $59dd h, $2d62 h, $3ceb h, $0e70 h, $1ff9 h,
  $f78f h, $e606 h, $d49d h, $c514 h, $b1ab h, $a022 h, $92b9 h, $8330 h,
  $7bc7 h, $6a4e h, $58d5 h, $495c h, $3de3 h, $2c6a h, $1ef1 h, $0f78 h,
decimal

0 variable packetNum
6  buffer: myAddr
6  buffer: smaAddr

: ^1b ( u -- )
  ;
: ^2b ( u -- ) dup ^1b 8 rshift ^1b ;
: ^4b ( u -- ) dup ^2b 16 rshift ^2b ;
: ^8b ( u1 u2 -- ) swap ^4b ^4b ;
: ^nb ( p n -- ) 0 ?do dup c@ ^1b 1+ loop drop ;

: ^m ( -- ) myAddr  6 ^nb ;
: ^s ( -- ) smaAddr 6 ^nb ;
: ^f ( -- ) $FFFFFFFF dup ^4b ^2b ;
: ^z ( -- ) 0 ^4b ;
: ^x ( -- ) $00020080 ^4b ;
: ^p ( -- ) $B8B8B8B8 ^4b $88888888 dup ^4b ^4b ;

: ^/ ( n1 n2 n3 -- )
  ;

: expect ( n -- f )
  ;

: start ( -- )
  ;

: sendPacket ( -- )
  ;
: sendAndWait ( -- )
  ;

: smaLogin ( -- n )
  $0002 expect not if 1 exit then
  \ copy smaAddr from returned data
  start ^m ^s $04000002 ^4b $0070 ^2b ( ??? ) ^1b ^z $00000001 ^4b sendPacket
  $000A expect not if 2 exit then
  $0005 expect not if 2 exit then
  start ^m ^f $00 $0000 $A009 ^/ ^x $00 ^1b ^z ^z sendAndWait
  start ^m ^f $03 $0300 $A008 ^/ $FD010E80 $FFFFFFFF ^8b $FF ^1b sendPacket
  1 packetNum +!
  start ^m ^f $01 $0100 $A00E ^/ $FD040C80 $000007FF ^8b
                                 $00038400 $BBAAAA00 ^8b $00 ^1b
                                 ^z ^p sendAndWait
  0 ;

: smaYield ( -- n )
  start ^m ^f $00 $0000 $A009 ^/ $26220054 $2622FF00 ^8b $00 ^1b sendAndWait
  \ copy result to ptime
  11111 ( ??? ) ;

: smaTotal ( -- n )
  start ^m ^f $00 $0000 $A009 ^/ $26010054 $2622FF00 ^8b $00 ^1b sendAndWait
  22222 ( ??? ) ;

: smapower ( -- n )
  start ^m ^f $00 $0000 $A109 ^/ ^x $263F0051 $263FFF00 ^8b $0E ^1b sendAndWait
  33333 ( ??? ) ;

: try2
  false bt-init
  s" $$$" uart-s.         listen-hex
  s" C"   uart-s. uart-cr listen-hex
  s" F,1" uart-s. uart-cr listen-hex
  BT-RESET ioc! ;

\ 1234 ms try
