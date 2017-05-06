\ application setup and main loop
\ reset
cr

: gr:line line ;

\ include ../flib/spi/lora1276.fs
include hc05.fs

: n-flash ( n -- ) \ flash LED n times very briefly (100ms)
  0 ?do
    LED ioc! 100 ms \ 1 low-power-sleep
    LED ios! 100 ms \ 1 low-power-sleep
  loop ;

: empty-stack ( any -- ) \ checks that the stack is empty, if not flash LED and empty it
  depth if
    rx-connected? if
      ." *** " depth . ." left on stack: " .v
    else
      10 n-flash
    then
    depth 100 u< if
      depth 0 do drop loop
    then
  then ;

: >gr ( c-addr len -- ) \ print a string to graphics
  0 ?do
    dup c@ ascii>bitpattern drawcharacterbitmap 1+
  loop  drop ;

: hms ( n -- c-addr len ) \ convert gps hour-minute-sec to string with : sep
  0 <# # # [char] : hold # # [char] : hold # # #> ;
: dmy ( n -- c-addr len ) \ convert gps day/month/year to string with / sep
  dup 10000 / swap 10000 mod ( d my )
  dup 100 mod dup 10000 * -rot - ( d y00 m0 )
  + + ( ymd )
  0 <# # # [char] / hold # # [char] / hold # # [char] 0 hold [char] 2 hold #> ;

: rf>oled ( len -- len ) \ show reception params on oled
  \ pkt format, source node
  0 font-x ! 0 font-y !
  dup 2 s>n >gr s" b " >gr
  rf.snr @ >n >gr s" dB " >gr
  rf.rssi @ >n >gr s" dBm " >gr
  rf.fei @ >n >gr s" Hz" >gr
  ;

10 cells buffer: gps-data

: gps-decode ( len -- ) \ decode GPS packet
  rf.buf 2+ swap var-init
  gps-data begin
    var> while
    over !
  cell+ repeat
  drop ;

: n>f4 ( n -- ncomma nwhole ) \ convert n with 4 frac digits to double
  $80000000 swap 10000,0 f/ ;
: n>f3 ( n -- ncomma nwhole ) \ convert n with 4 frac digits to double
  $80000000 swap 1000,0 f/ ;

: gps>oled ( -- ) \ display some GPS info on oled
  0 font-x ! 16 font-y !  gps-data
  dup 6 cells + @ dmy  >gr s"  " >gr \ date
  dup 0 cells + @ 1000 / hms >gr s"  " >gr \ time
  dup 1 cells + @ ascii>bitpattern drawcharacterbitmap \ flag
  0 font-x ! 32 font-y !
  dup 2 cells + @ n>f4 4 4 f>n.m >gr s"  " >gr \ latitude
  dup 3 cells + @ n>f4 6 4 f>n.m >gr s"  " >gr \ longitude
  0 font-x ! 48 font-y !
  dup 4 cells + @ n>f3 0 1 f>n.m >gr s" kts" >gr \ knots
  drop ;

: gps>uart ( -- ) \ print GPS info
  gps-data 5 spaces
  dup 6 cells + @ dmy type space
  dup 0 cells + @ 1000 / hms type space
  dup 1 cells + @ emit ."  -- "
  dup 2 cells + @ n>f4 0 4 f>n.m type space
  dup 3 cells + @ n>f4 0 4 f>n.m type space
  dup 4 cells + @ n>f3 0 1 f>n.m type space
  drop cr ;

: str.u2 send-string ;
: str,u2 send-string [char] , uart2-emit ;
: gpgga>uart2 ( -- c-addr len ) \ generate gpgga GPS record from avail data
  s" $GPGGA" str,u2 gps-data
  dup 0 cells + @ 1000 / >n str,u2 \ time
  dup 2 cells + @ dup abs n>f4 0 4 f>n.m str,u2 \ latitude
    0< if s" S," else s" N," then str.u2
  dup 3 cells + @ dup abs n>f4 0 4 f>n.m str,u2 \ longitude
    0< if s" W," else s" E," then str.u2
  dup 1 cells + @ [char] A = if s" 1" else s" 0" then  str,u2 \ fix quality
  s" ,,,,,*00" send-line \ misc fields
  drop
  ;

: init-hw
  16MHz 1000 systick-hz
  lptim-init

  uart2-init
  38400 uart2-baud

  lcd-init show-logo 

  \ LoRa @423.6Mhz
  432595 $CB rf-init rf!lora125.8
  17 rf-power
  ;

: main
  init-hw begin
    rf-recv ?dup if
      rf-ack
      ." LoRa RCV " rf>uart cr
      \ ." : " dup 0 do rf.buf i + c@ . loop cr
      clear rf>oled 
      \ 0  0 127  0 gr:line
      \ 0 63 127 63 gr:line
      gps-decode gps>oled display
      gps>uart
      gpgga>uart2
    then
    empty-stack
    1 ms
  again ;

\ main
