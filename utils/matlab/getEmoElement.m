function [b,e,v]=getEmoElement(fname,el)
 b=[];e=[];v=[];
 import java.io.*;
 import javax.xml.stream.*;
 import javax.xml.parsers.*;
 fileInputStream = FileInputStream(fname);
 xmlStreamReader = XMLInputFactory.newInstance().createXMLStreamReader(fileInputStream);
 while xmlStreamReader.hasNext()
     eventCode = xmlStreamReader.next();
     if (XMLStreamConstants.START_ELEMENT == eventCode) ...
         && xmlStreamReader.getLocalName().equalsIgnoreCase('p') 
        b=[b datevec(char(xmlStreamReader.getAttributeValue('','begin')), 'HH:MM:SS.FFF')*[0,0,0,3600,60,1]'];
        e=[e datevec(char(xmlStreamReader.getAttributeValue('','end')), 'HH:MM:SS.FFF')*[0,0,0,3600,60,1]'];
        while xmlStreamReader.hasNext()
             eventCode = xmlStreamReader.next();

      % this breaks _users record_ reading logic
      
             if (XMLStreamConstants.END_ELEMENT == eventCode) ...
                && xmlStreamReader.getLocalName().equalsIgnoreCase('p')
                break;          
             elseif (XMLStreamConstants.START_ELEMENT == eventCode) ...
                    && xmlStreamReader.getLocalName().equalsIgnoreCase('data')
                 while xmlStreamReader.hasNext()
                    eventCode = xmlStreamReader.next();
                    if (XMLStreamConstants.END_ELEMENT == eventCode) ...
                       && xmlStreamReader.getLocalName().equalsIgnoreCase('data')
                        break;          
                    elseif (XMLStreamConstants.START_ELEMENT == eventCode) ...
                           && xmlStreamReader.getLocalName().equalsIgnoreCase('metadata')
                        while xmlStreamReader.hasNext()
                            eventCode = xmlStreamReader.next();
                            if (XMLStreamConstants.END_ELEMENT == eventCode) ...
                                && xmlStreamReader.getLocalName().equalsIgnoreCase('metadata')
                                break;
                            end
                        end        
                        eventCode = xmlStreamReader.next();                       
                        if (XMLStreamConstants.CDATA == eventCode||XMLStreamConstants.CHARACTERS == eventCode) 

                  % extract the user data
                  %{
                            cxml=xmlStreamReader.getText();
                            stream = java.io.StringBufferInputStream(cxml);
                            factory = DocumentBuilderFactory.newInstance();
                            builder = factory.newDocumentBuilder;
                            document = builder.parse(stream);
                   %}       
                            try
                                cxml=xmlStreamReader.getText();
                                stream = java.io.StringBufferInputStream(cxml);
                                cdatareader = XMLInputFactory.newInstance().createXMLStreamReader(stream);
                                while cdatareader.hasNext()
                                    eCode = cdatareader.next();
                                    if (XMLStreamConstants.START_ELEMENT == eCode) ...
                                        && cdatareader.getLocalName().equalsIgnoreCase(el) 

                                        while cdatareader.hasNext()
                                            eCode = cdatareader.next();
                                            if (XMLStreamConstants.END_ELEMENT == eCode) ...
                                                && cdatareader.getLocalName().equalsIgnoreCase(el)
                                                break;          
                                            elseif (XMLStreamConstants.CHARACTERS == eCode) 
                                                v=[v str2double(cdatareader.getText())];
                                                %plot(b,v);
                                            end
                                        end
                                    end
                                end
                            catch 
                                v=[v nan];
                            end
                        end
                     end
                 end
             end
        end
     end
 end
