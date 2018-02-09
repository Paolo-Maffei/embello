\ PDP-8 emulator
forgetram

\ led-init
\ sd-mount.
\ ls

8192 buffer: M       \ simulated memory
ROM M ROM-SIZE move  \ initial contents

0 variable AC
0 variable PC
0 variable IR
0 variable MQ

: octal ( --) #8 base ! ;  octal  \ switch to octal mode from here on down

: m@ ( u -- u )  shl M + h@ ;
: m! ( u1 u2 -- )  shl M + h! ;
: ac@ ( -- u )  AC @ ;
: ac! ( u -- )  AC ! ;
: pc@ ( -- u )  PC @ ;
: pc! ( u -- )  PC ! ;

: maskw ( u -- u ) 07777 and ;
: maskl ( u -- u ) 17777 and ;
: keepl ( u -- u ) 10000 and ;
: 1+w   ( u -- u ) 1+ maskw ;
: ++pc  ( -- )     pc@ 1+w pc! ;

: addr ( u -- u )
  dup 0177 and  \ immediate
  over 0200 and if
    pc@ 1- 7600 or  \ page-relative
  then
  swap 0400 and if
    dup 7770 and 0010 = if dup m@ 1+w over m! then  \ auto-inc
    m@  \ indirect
  then ;

: op0 ( u -- ) addr m@  10000 or  ac@ and ac! ;              \ AND
: op1 ( u -- ) addr m@  ac@ + maskl ac! ;                    \ TAD
: op2 ( u -- ) addr  dup m@ 1+w  dup rot m!  if ++pc then ;  \ ISZ
: op3 ( u -- ) addr  ac@  dup maskw  rot m!  keepl ac! ;     \ DCA
: op4 ( u -- ) addr  pc@ over m!  1+w pc! ;                  \ JMS
: op5 ( u -- ) addr pc!  ;                                   \ JMP

: op6 ( u -- )  \ IOT
    dup  3 rshift 077 and case
      03 of dup 1 and if key? if ++pc then then  \ skip if input ready
                4 and if key? if key else 0 then ac@ keepl or ac! then  \ rdch
      endof
      04 of dup 1 and if ++pc then  \ skip if output ready
            dup 4 and if ac@ 0177 and emit then  \ wrch
                2 and if ac@ keepl ac! then  \ clear flag
      endof
    endcase  drop ;

: op7g1 ( u -- )  \ OPR
  ac@  ( ir ac )
  over 0200 and if     keepl then  \ CLA
  over 0100 and if     maskw then  \ CLL
  over 0040 and if 07777 xor then  \ CMA
  over 0020 and if 10000 xor then  \ CML
  over 0001 and if 1+  maskl then  \ IAC
  swap 0016 and case
   12 of dup shl shl maskw swap       #11 rshift or        endof  \ RTR
   10 of dup shl     maskw swap       #12 rshift or        endof  \ RAR
   06 of dup shr shr       swap 3 and #11 lshift or        endof  \ RTL
   04 of dup shr           swap 1 and #12 lshift or        endof  \ RAL
   02 of dup keepl over 6 lshift maskw or swap 6 rshift or endof  \ BSW
  endcase
  ac! ;

: op7g2 ( u -- )  \ OPR
;

: op7g3 ( u -- )  \ OPR
  MQ @ swap  ( mq ir )
  dup 0200 and if ac@ keepl ac! then  \ CLA
  dup 0020 and if ac@ dup maskw MQ ! keepl ac! then  \ MQL
      0100 and if ac@ or ac! else drop then ;

: op7 ( u -- )  \ OPR
  dup 0400 and 0= if op7g1 else
  dup 0001 and 0= if op7g2 else
                     op7g3 then then ;

create op-tab ' op0 , ' op1 , ' op2 , ' op3 , ' op4 , ' op5 , ' op6 , ' op7 , 

: cycle
  pc@  dup 1+w pc!  m@
  dup #8 rshift %1100 and  op-tab + @  execute ;

0200 pc!

decimal
