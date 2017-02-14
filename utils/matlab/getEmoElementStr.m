function [b,e,v]=getEmoElementStr(fname,el)
 b=[];e=[];v=[];
 import java.io.*;
 import javax.xml.stream.*;
 import javax.xml.parsers.*;
 [XMLStreamConstants.START_ELEMENT XMLStreamConstants.END_ELEMENT XMLStreamConstants.CDATA XMLStreamConstants.CHARACTERS]
 fileInputStream = FileInputStream(fname);
 xmlStreamReader = XMLInputFactory.newInstance().createXMLStreamReader(fileInputStream);
 while xmlStreamReader.hasNext()
     eventCode = xmlStreamReader.next();
     if (XMLStreamConstants.START_ELEMENT == eventCode) ...
         && xmlStreamReader.getLocalName().equalsIgnoreCase('p') 
        b=[b xmlStreamReader.getAttributeValue('','begin')];
        e=[e xmlStreamReader.getAttributeValue('','end')];
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
                                                v=[v cdatareader.getText()];
                                            end
                                        end
                                    end
                                end
                            catch 
                                v=[v ''];
                            end
                        end
                     end
                 end
             end
        end
     end
 end
