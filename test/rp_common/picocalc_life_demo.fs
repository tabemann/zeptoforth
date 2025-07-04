begin-module life-demo

  oo import
  life import
  
  320 320 life-buf-size buffer: my-life-buf
  <life> class-size buffer: my-life
  
  : init-life ( -- }
    my-life-buf 320 320 <life> my-life init-object
  ;
  
  initializer init-life
  
end-module
