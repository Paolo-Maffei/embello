\ application setup and main loop
\ assumes that the GPS is connected to usart2 on PA2&PA3

0 constant debug  \ 0 = send RF packets, 1 = display on serial port
1 constant rate   \ seconds between readings

: s>usart2 ( c-addr len -- ) \ send string to uart2
  0 ?do dup i + c@ >usart2 loop drop ;

: s>gps ( c-addr len -- ) \ send command string to GPS
  s" PMTK"  s>usart2 s>usart2 13 >usart2 10 >usart2
  ;

: gps? \ print GPS info
  s" 605,*31" s>gps
  ;

: fwd
  begin usart2>? if usart2> emit then key? until ." OK" ;

usart2-init
