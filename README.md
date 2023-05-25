# CodeNumberScraper

Scrapes the Websites of the DVGW and the BDEW for Marketpartner-Codenumbers

The result is an CSV file with the following header:

Code;Kind;CompanyName;Role;ValidFrom;InternalCode

Code: The Code of the marketpartner
Kind: Dvgw or Bdew
CompanyName: The name of the company holding the code (trailing whitespaces and quaotation marks are removed)
Role: market role
ValidFrom: Only works for DVGW - date from which the code was granted
InternalCode: The Id used for the company internally in the dvgw or BDEW

Just simply compile and run and it will download all the data and write it into a marketpartner.csv

The output path can be configured through a start argument --out
E.g.: `CodeNumberScraper --out c:\temp\marketpartners.csv`
