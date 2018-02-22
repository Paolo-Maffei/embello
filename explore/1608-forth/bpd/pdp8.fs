\ PDP-8 emulator

8192 buffer: mem  \ simulated memory, 4 Kw = 8 Kb

0 variable ac.r  \ accumulator
0 variable pc.r  \ program counter
0 variable mq.r  \ memory quotient
0 variable sr.r  \ switch register
0 variable iena  \ shifting interrupt enable

: octal ( --) #8 base ! ;  octal  \ switch to octal mode from here on down
: o.4 ( u -- ) base @ octal swap  u.4  base ! ;

: m@ ( u -- u )    shl mem + h@ ;  \ fetch memory word
: m! ( u1 u2 -- )  shl mem + h! ;  \ store memory word

$00010000 constant DF32-BASE
0 variable daddr
: wc.r 7750 shl mem + ;  \ df32 word count
: ma.r 7751 shl mem + ;  \ df32 memory address

: ac  ( -- u )  ac.r @ ;  \ fetch accumulator
: ac! ( u -- )  ac.r ! ;  \ save in accumulator
: pc  ( -- u )  pc.r @ ;  \ fetch program counter
: pc! ( u -- )  pc.r ! ;  \ save in program counter

: low12 ( u -- u ) 07777 and ;   \ clear link bit
: low13 ( u -- u ) 17777 and ;   \ clear overflow above link bit
: clr12 ( u -- u ) 10000 and ;   \ clear accumulator, keep link bit
: 1+w   ( u -- u ) 1+ low12 ;    \ increment modulo 4096
: ++pc  ( -- )     pc 1+w pc! ;  \ advance program counter

: addr ( u -- u )  \ common instruction decode, converts ir to effective addr
  dup 0177 and  ( ir ac )                  \ immediate
  over 0200 and if pc 1- 7600 and or then  \ page-relative
  swap 0400 and if                         \ auto-inc, indirect
    dup 7770 and 0010 = if dup m@ 1+w over m! then  m@
  then ;

: op0 ( u -- ) addr m@  10000 or  ac and ac! ;                  \ AND
: op1 ( u -- ) addr m@  ac + low13 ac! ;                        \ TAD
: op2 ( u -- ) addr  dup m@ 1+w  dup rot m!  0= if ++pc then ;  \ ISZ
: op3 ( u -- ) addr  ac  dup low12  rot m!  clr12 ac! ;         \ DCA
: op4 ( u -- ) addr  pc over m!  1+w pc! ;                      \ JMS
: op5 ( u -- ) addr pc!  ;                                      \ JMP

: op6 ( u -- )  \ IOT
    dup  3 rshift 077 and case
      00 of dup 1 and if -2 iena ! then  \ lower bit is 0, i.e. 1-cycle delay
            dup 2 and if  0 iena ! then
      endof
      03 of dup 1 and if key? if ++pc then then  \ skip if input ready
            dup 4 and if key? if key else 0 then ac clr12 or ac! then  \ rdch
      endof
      04 of dup 1 and if ++pc then  \ skip, output always ready
            dup 4 and if ac 0177 and emit then  \ wrch
            dup 2 and if ac clr12 ac! then  \ clear flag
      endof
      60 of  \ DF32
        cr ." IOT " dup o.4 space
        dup 1 and if 0 daddr ! then
        dup 2 and if  \ read
          daddr @ 2* DF32-BASE +     \ from flash
          10000 wc.r h@ -            \ word count
          ma.r h@ 2dup + ma.r h! 2+  \ advance memory address
          2* mem +                   \ to virtual ram
          swap 2* move               \ copy bytes
          0 wc.r h!                  \ clear count
        then
        dup 4 and if  \ write (dummy)
          10000 wc.r h@ -            \ word count
          ma.r h@ + ma.r h! 2+       \ advance memory address
          0 wc.r h!                  \ clear count
        then
      endof
      61 of  \ DF32
        cr ." IOT " dup o.4 space
      endof
      62 of  \ DF32
        cr ." IOT " dup o.4 space
        dup 2 and if ++pc then  \ skip if done
      endof
      \ cr ." IOT " over o.4
    endcase  drop ;

: op7g1 ( u -- )  \ OPR
  ac  ( ir ac )
  over 0200 and if     clr12 then  \ CLA
  over 0100 and if     low12 then  \ CLL
  over 0040 and if 07777 xor then  \ CMA
  over 0020 and if 10000 xor then  \ CML
  over 0001 and if 1+  low13 then  \ IAC
  swap 0016 and case
   02 of dup clr12 over 6 lshift low12 or swap 6 rshift or endof  \ BSW
   04 of dup shl     low13 swap       #12 rshift or        endof  \ RAR
   06 of dup shl shl low13 swap       #11 rshift or        endof  \ RTR
   10 of dup shr           swap 1 and #12 lshift or        endof  \ RAL
   12 of dup shr shr       swap 3 and #11 lshift or        endof  \ RTL
  endcase
  ac! ;

: op7g2 ( u -- )  \ OPR
  0  ( ir f )
  over 0100 and if ac 04000 and or then  \ SMA, SPA
  over 0040 and if ac low12 0=  or then  \ SZA, SNA
  over 0020 and if ac clr12     or then  \ SNL, SZL
  over 0010 and if              0= then  \ reverse
  if ++pc then  \ skip if above condition is met
  dup 0200 and if ac clr12     ac! then  \ CLA
  dup 0004 and if ac sr.r @ or ac! then  \ OSR
      0002 and if cr ." HALT" quit then  \ HLT
;

: op7g3 ( u -- )  \ OPR
  mq.r @ swap  ( mq ir )
  dup 0200 and if ac                  clr12 ac! then  \ CLA
  dup 0020 and if ac dup low12 mq.r ! clr12 ac! then  \ MQL
      0100 and if ac or ac! else drop then ;

: op7 ( u -- )  \ OPR
  dup 0400 and 0= if op7g1 else
  dup 0001 and 0= if op7g2 else
                     op7g3 then then ;

create op-tab ' op0 , ' op1 , ' op2 , ' op3 , ' op4 , ' op5 , ' op6 , ' op7 , 

: cycle ( -- )  \ execute one instruction
  iena @ 2/ iena !  \ bit 0 determines whether interrupts are enabled
  pc  dup 1+w pc!  m@
  dup 7 rshift %11100 and  op-tab + @  execute ;

: go  \ load program and run forever
  [ifdef] ROM
    ROM mem ROM-SIZE move  \ copy rom data to ram
  [else]
    7600 0200 m!
    6603 0201 m!
    6622 0202 m!
    5202 0203 m!
    5600 0204 m!
    7576 wc.r m!
    7576 ma.r m!
  [then]
  0200 pc!
  begin
    #1000 0 do
      cr pc o.4 ." : " pc m@ o.4 space ac #12 rshift [char] 0 + emit ac o.4
      cycle
    loop
    \ simulate periodic timer with an interrupt every 1000 cycles
    iena @ 1 and if  pc 0 m!  1 pc!  0 iena !  then
  again ;

decimal
\ go
