\ access HC05 Bluetooth module via UART2, for use with SMA solar inverter

compiletoram? [if]  forgetram  [then]

 0 variable fcs
 0 variable escaped
 0 variable pNum
 0 variable pFill

150 buffer: pBuf
  6 buffer: smaAddr

$2013 $10220486 2variable myAddr

\ myAddr 6 dump
\ smaAddr 6 dump

: fcsUpdate ( b -- )
\ fcsCheck = (fcsCheck >> 8) ^ fcstab[(byte) fcsCheck ^ b];
  fcs @ xor $FF and  shl fcstab + h@  fcs @ 8 rshift  xor fcs ! ;
\ fcs @ tuck 8 rshift -rot  xor  $FF and  shl fcstab + h@  xor fcs ! ;

: ^byte ( b -- )  \ append byte to packet buffer
  pFill @ c!  1 pFill +! ;

: ?b ( -- b )  \ get one byte from uart, save in packet buffer
  uart-irq-key  dup ^byte ;
\ fcs @ xor fcs ! ;

: readPacket ( -- )
  \ TODO ignore $FF counts!
  begin  pBuf pFill !  ?b $7E = until
  ?b  ." P:" dup .
  dup $FF = if
    pbuf 150 $AA fill  \ not a valid packet
  else
    0 swap 150 min 2-  0 do
      uart-irq-key  ( flip uart )
      dup $7D = if
        2drop $20
      else
        xor ^byte 0
      then
    loop
  then drop ;

: check ( -- f )
  $FFFF fcs !  pFill @ pBuf - 3 -
  ." N:" dup . dup 19 > if
    19 do
      i pBuf + c@ fcsUpdate
    loop
  else
    drop
  then fcs @ pFill @ 3 - c@ pFill @ 2- c@ 8 lshift or xor ;

: pType ( -- n )  pBuf 16 + h@ ;

: readPacket# ( -- )  \ debug version, prints out contents
  pbuf 150 $FF fill  readPacket  pType h.4 space  check h.4 space  depth .
  pBuf pBuf 1+ c@ dump ;

: expect ( n -- f )
  readPacket#  pType = ;

: ^1 ( u -- )
  escaped @ if
    dup fcsUpdate
    dup case
      $7D of 1 endof
      $7E of 1 endof
      $11 of 1 endof
      $12 of 1 endof
      $13 of 1 endof
             0 swap
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
\ emitInt(0x0001, 2);
\ emitOne(0x7E);
\ escaped = true;
\ emitInt(0x656003FFL, 4);
\ emitInt(va_arg(ap, int), 2);
\ emitBytes(allffs, 6);
\ emitInt(va_arg(ap, int), 2);
\ byte fakeAddr[] = { 0x5C,0xAF,0xF0,0x1D,0x50,0x00 };
\ emitBytes(fakeAddr, 6);
\ emitOne(0x00);
\ emitOne(va_arg(ap, int));
\ emitInt(0L, 4);
\ emitOne(packetNum);
  $0001 ^2 $7E ^1
  true escaped !
  $656003FF ^4 ( n3 ) ^2 ^f ( n2 ) ^2  $1DF0AF5C ^4 $0050 ^2  $00 ^1 ( n1 ) ^4
  pNum @ ^1 ;

: emitFinal ( -- )
  pFill @ pBuf - pBuf 1+ c!  \ pBuf[1] = pFill - pBuf
  pBuf h@ dup 8 rshift xor  pBuf 3 + c!  \ pBuf[3] = pBuf[0] ^ pBuf[1]
  ." emit:" pFill @ pBuf do i c@ h.2 loop cr
  pFill @ pBuf do i c@ uart-emit loop ;

: start ( -- )
  $FFFF fcs !
  pBuf pFill !
  false escaped !
  $007E ^4 ;

: sendPacket ( -- )
  escaped @ if
    false escaped !
    fcs @ not ^2
    $7E ^1
  then
  emitFinal ;

: smaLogin ( -- n )
  0 pNum !
  $0002 expect not if 1 exit then
  pBuf 4 + smaAddr 6 move
  pBuf 22 + c@ ( seq# ? )
  1001 .
  start ^m ^s $04000002 ^4 $0070 ^2 ^1 ^z $00000001 ^4  sendPacket
  $000A expect not if 2 exit then
  begin $0005 expect until
  1002 .
  begin
    start ^m ^f $00 $0000 $A009 ^/ ^x $00 ^1 ^z ^z  sendPacket
  $0001 expect  pBuf 45 + c@ pNum c@ =  and until
  1 pNum +!
  1003 .
  start ^m ^f $03 $0300 $A008 ^/ $FD010E80 $FFFFFFFF ^8 $FF ^1 sendPacket
  1 pNum +!
  1004 .
  begin
    start ^m ^f $01 $0100 $A00E ^/ $FD040C80 $000007FF ^8
                                   $00038400 $BBAAAA00 ^8 $00 ^1
                                   ^z ^p sendPacket
  $0001 expect  pBuf 45 + c@ pNum c@ =  and until
  1 pNum +!
  1005 .
  0 ;

 0 variable result

: copyResult ( -- )  pBuf 67 + result 4 move ;

: smaPower ( -- n )
\ sendAndWait(PSTR("$m $f / $x 51003F2600FF3F26000E"), 0xA109, 0, 0);
  begin
    start ^m ^f $00 $0000 $A109 ^/ ^x $263F0051 $263FFF00 ^8 $0E ^1 sendPacket
  $0001 expect  pBuf 45 + c@ pNum c@ =  and until
  1 pNum +!
  copyResult ;

: smaYield ( -- n )
  begin
    start ^m ^f $00 $0000 $A009 ^/ $26220054 $2622FF00 ^8 $00 ^1 sendPacket
  $0001 expect  pBuf 45 + c@ pNum c@ =  and until
  1 pNum +!
  copyResult ;

: smaTotal ( -- n )
\ sendAndWait(PSTR("$m $s / $x 5400012600FF012600"), 0xA009, 0, 0);
  begin
    start ^m ^f $00 $0000 $A009 ^/ $26010054 $2622FF00 ^8 $00 ^1 sendPacket
  $0001 expect  pBuf 45 + c@ pNum c@ =  and until
  1 pNum +!
  copyResult ;

: try
  cr false bt-init
\ readPacket#
  cr smaLogin ." login:" .
  cr smaPower ." power: " result h@ .
  cr smaYield ." yield: " result h@ .
  cr smaTotal ." total: " result @ .
  BT-RESET ioc! ;

\ 1234 ms try
