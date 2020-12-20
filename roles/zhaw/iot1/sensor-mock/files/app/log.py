#!/usr/bin/env python3

'''

                    ___           ___           ___     
        ___        /\__\         /\  \         /\  \    
       /\  \      /::|  |       /::\  \       /::\  \   
       \:\  \    /:|:|  |      /:/\:\  \     /:/\ \  \  
       /::\__\  /:/|:|  |__   /::\~\:\  \   _\:\~\ \  \ 
    __/:/\/__/ /:/ |:| /\__\ /:/\:\ \:\__\ /\ \:\ \ \__\
   /\/:/  /    \/__|:|/:/  / \:\~\:\ \/__/ \:\ \:\ \/__/
   \::/__/         |:/:/  /   \:\ \:\__\    \:\ \:\__\  
    \:\__\         |::/  /     \:\ \/__/     \:\/:/  /  
     \/__/         /:/  /       \:\__\        \::/  /   
                   \/__/         \/__/         \/__/    


    File:          log.py
    

    Purpose:       Custom Logger to put debug / log
                   messages to a directive by using
                   a suitable logging handler.
                   
    
    Remarks:       - Script execution requires
                     the python default 
                     logging module.
    

    Author:        P. Leibundgut <leiu@zhaw.ch>
    

    Date:          09/2016


'''

import sys
import logging


def setup_custom_logger( name ):

  logging.basicConfig( level = logging.INFO )
  logger = logging.getLogger( name )
  logger.setLevel( logging.DEBUG )
  
  return logger
