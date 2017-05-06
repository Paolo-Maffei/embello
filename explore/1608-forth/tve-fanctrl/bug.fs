<<<board>>>
cr compiletoflash

: hello ." hello" cr ;

: init init
  ." hello is at "
  s" hello" find drop hex. space
  ' hello hex. cr
  ;

." hello is at " ' hello hex. cr

compiletoram
