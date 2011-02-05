-- Here is implementation of override to 'print' function
--
print("redirecting output...")
function print( message )
  app.addConsoleMessage( "> " .. tostring( message ) ); 	
end;
print("Console ready... type 'help' to learn more about the console.")


-- This code parses the input to the console
--
function parse( msg )
  msg = tostring(msg);
  _,_,command,parameter = string.find(msg, "(%w+)%s(.*)");

  if command == "quit" then
    app.quit();
  end;

  if command == "print" then
    print(parameter);
  end;

  if command == "help" then
    showHelp();
  end;

  if (command == "script") and (parameter ~= "") then
    dofile(parameter);
  end;

end;

-- This function shows the usable commands for the console
--
function showHelp()
  print( "The following commands are valid:" );
  print( "help - Show this help");
  print( "print [message] - Print a message to the console" );
  print( "quit - Shut down the application");
end;