\ access HC05 Bluetooth module via UART2, for use with SMA solar inverter

compiletoram? [if]  forgetram  [then]

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

: try
  false bt-init
  s" $$$" uart-s.         listen-hex
  s" C"   uart-s. uart-cr listen-hex
  s" F,1" uart-s. uart-cr listen-hex
  BT-RESET ioc! ;

\ 1234 ms try
