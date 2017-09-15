\ access HC05 Bluetooth module via UART2, for use with SMA solar inverter

compiletoram? [if]  forgetram  [then]

 0 variable fcs
 0 variable escaped
 0 variable pNum
 0 variable pFill

150 buffer: pBuf
  6 buffer: myAddr
  6 buffer: smaAddr

: fcsUpdate ( b -- )
  fcs @ tuck 8 rshift -rot  xor  $FF and  shl fcstab + h@  xor fcs ! ;

: ^byte ( b -- )  pFill @ c!  1 pFill +! ;

: ^1 ( u -- )
  escaped @ if
    dup fcsUpdate
    dup case
      $7D of 1 endof
      $7E of 1 endof
      $11 of 1 endof
      $12 of 1 endof
      $13 of 1 endof
             0
    endcase
    if $7D ^byte $20 xor then
  then ^byte ;

: ^2 ( u -- )  dup ^1 8 rshift ^1 ;
: ^4 ( u -- )  dup ^2 16 rshift ^2 ;
: ^8 ( u1 u2 -- )  swap ^4 ^4 ;
: ^n ( p n -- )  0 ?do dup c@ ^1 1+ loop drop ;

: ^m ( -- )  myAddr  6 ^n ;
: ^s ( -- )  smaAddr 6 ^n ;
: ^f ( -- )  $FFFFFFFF dup ^4 ^2 ;
: ^z ( -- )  0 ^4 ;
: ^x ( -- )  $00020080 ^4 ;
: ^p ( -- )  $B8B8B8B8 ^4 $88888888 dup ^8 ;

: ^/ ( n1 n2 n3 -- )
  $0001 ^2 $7E ^1
  true escaped !
  $656003FF ^4 ^2 ^f ^2  $1DF0AF5C ^4 $0050 ^2  $00 ^1 ^4
  pNum @ ^1 ;

: emitFinal ( -- )
  pFill @ pBuf @ - pBuf @ 1+ c!  \ pBuf[1] = pFill - pBuf
  pBuf @ h@ dup 8 lshift xor  pBuf @ 3 + c!  \ pBuf[3] = pBuf[0] ^ pBuf[1]
  pFill @ pBuf @ do i c@ uart-emit loop ;

: start ( -- )
  false escaped !
  $FFFF fcs !
  pBuf pFill !
  $007E ^4 ;

: sendPacket ( -- )
  escaped @ if
    false escaped !
    fcs @ not ^2
    $7E ^1
  then
  emitFinal ;

: expect ( n -- f )
  ;

: sendAndWait ( -- )
  \ ...
  1 pNum +! ;

: smaLogin ( -- n )
  $0002 expect not if 1 exit then
  \ copy smaAddr from returned data
  start ^m ^s $04000002 ^4 $0070 ^2 ( ??? ) ^1 ^z $00000001 ^4 sendPacket
  $000A expect not if 2 exit then
  $0005 expect not if 2 exit then
  start ^m ^f $00 $0000 $A009 ^/ ^x $00 ^1 ^z ^z sendAndWait
  start ^m ^f $03 $0300 $A008 ^/ $FD010E80 $FFFFFFFF ^8 $FF ^1 sendPacket
  1 pNum +!
  start ^m ^f $01 $0100 $A00E ^/ $FD040C80 $000007FF ^8
                                 $00038400 $BBAAAA00 ^8 $00 ^1
                                 ^z ^p sendAndWait
  0 ;

: smaPower ( -- n )
  start ^m ^f $00 $0000 $A109 ^/ ^x $263F0051 $263FFF00 ^8 $0E ^1 sendAndWait
  33333 ( ??? ) ;

: smaYield ( -- n )
  start ^m ^f $00 $0000 $A009 ^/ $26220054 $2622FF00 ^8 $00 ^1 sendAndWait
  \ copy result to ptime
  11111 ( ??? ) ;

: smaTotal ( -- n )
  start ^m ^f $00 $0000 $A009 ^/ $26010054 $2622FF00 ^8 $00 ^1 sendAndWait
  22222 ( ??? ) ;

: try
  false bt-init
  s" $$$" uart-s.         listen-hex
  s" C"   uart-s. uart-cr listen-hex
  s" F,1" uart-s. uart-cr listen-hex
  BT-RESET ioc! ;

\ 1234 ms try
