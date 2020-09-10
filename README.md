# qr-game

QR Game is a web application made with ASP.NET Core MVC. Tha main purpose of this application is creation and participation in treasure hunt like quiz game based on scanning QR codes.

Users are divided into 3 main groups based on privileges:

**Administator** - Oversees user accounts and has ability to move users between groups 
<br/>
**Moderators** - Moderators can create questions and contests
<br/>
**Contestants (regular users)** - Participate in contests by finding hidden qr code, scanning it and answering question
<br/>
<br/>

**Questions** 
- Each question can have a variable number of answers, which can contain text and image.
Questions are kept separately so they can be used in multiple contests without needing to create them again

**Contests**
- Every contest have two states (active, inactive), which indicates whether contestants can view contest questions. It is also possible to generate qr codes for the contest, which are formatted so that you only need to print them

**Answers**
- After contest, user answers can be exported directly to .xlsx file which contains summary report of each participant and their final score
