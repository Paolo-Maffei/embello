\ application setup and main loop
\ assumes that the GPS is connected to usart2 on PA2&PA3
reset
cr

\ include ../flib/spi/lora1276.fs
include gps.fs

: init-hw
  16MHz 1000 systick-hz
  uart2-init
  lptim-init

  \ 915750 rf.freq ! 6 rf.group ! 62 rf.nodeid !
  \ rf-init 16 rf-power

  \ LoRa @423.6Mhz
  432592 $CB rf-init rf!bw10cr6sf7
  ;

66 buffer: txbuf
0 variable txlen

: get-gps init-hw begin gps-line ?dup 0<> until txbuf -rot buffer-cpy ;

: tx+ack ( c-addr len -- )
  txbuf -rot buffer-cpy ( txbuf len )        \ copy from print buffer to tx buffer
  over $80 swap cbis!   ( txbuf len )        \ set info bit in format byte
  2dup + rf+info        ( txbuf len )        \ append rf info trailer
  2+                    ( txbuf len+2 )      \ adjust for info trailer
  ." BUF> " 2dup buffer. cr
  led-off
  $2A rf-send           ( )                  \ send with ack req as node 10
  rf-ack? ?dup if                            \ wait for ack
    led-on ." LoRa ACK " rf>uart ." : " . cr
  else
    ." Lost" cr
  then ;

: main
  init-hw begin
    gps-line ?dup if
      \ ." GPS> " 2dup buffer. cr
      tx+ack
    then
  key? until ;

\ main
