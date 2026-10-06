# بنية النظام وشاشات الموظف

## 1. مسؤولية كل مشروع

| المشروع | الاستهداف والمراجع الأساسية | المسؤولية في الكود الحالي |
|---|---|---|
| [DVLD.csproj](<J:/gradeat project/DVLD Project Final/Project/DVLD/DVLD.csproj>) | `.NET Framework 4.8`، WinForms، `System.Net.Http`، مرجع `System.Text.Json` وحزمته `8.0.6` | واجهات الموظف، تحميل البيانات وعرضها، التحقق داخل الشاشة، بدء عمليات الوثائق والتوليد |
| [DVLD_Buisness.csproj](<J:/gradeat project/DVLD Project Final/Project/DVLD_Buisness/DVLD_Buisness.csproj>) | `net48;net10.0`، مرجع DataAccess | كائنات النظام، عمليات Save/Find، قواعد وإجراءات الرخص والأسئلة والامتحان |
| [DVLD_DataAccess.csproj](<J:/gradeat project/DVLD Project Final/Project/DVLD_DataAccess/DVLD_DataAccess.csproj>) | `net48;net10.0`، `Microsoft.Data.SqlClient 6.1.6` | SQL connections/commands/readers، الاستعلامات والإدراج والتعديل والحذف |
| [DVLD.Api.csproj](<J:/gradeat project/DVLD Project Final/Project/DVLD.Api/DVLD.Api.csproj>) | `net10.0`، مرجع Business وAI، `Microsoft.AspNetCore.OpenApi 10.0.12` | HTTP Controllers، تحويل البيانات إلى JSON، طوابير الخلفية، معالج الأخطاء |
| [DVLD.AI.csproj](<J:/gradeat project/DVLD Project Final/Project/DVLD.AI/DVLD.AI.csproj>) | `net10.0`، `PdfPig 0.1.16`، `Qdrant.Client 1.19.0`، مرجع `OpenAI 2.14.0` | استخراج النص، معالجة العربية، تقسيم المقاطع، embeddings والبحث، اتصال Ollama، اختيار مصادر الأسئلة والتحقق |

الاسم `DVLD_Buisness` مكتوب بهذه الصورة في المشروع؛ لذلك تُستخدم هذه التهجئة في المسارات. استهداف Business وDataAccess لإصدارين يسمح باستخدامهما من WinForms القديم ومن API الحديث. وجود حزمة `OpenAI` في المراجع لا يعني أن المسار الحالي يستخدم API مدفوعًا: خدمات التوليد والشات والـembeddings المفحوصة تتصل بـOllama المحلي.

الفرق بين الطبقات عملي: الشاشة تأخذ المدخلات وتعرض النتيجة، Business يمثل العملية، وDataAccess ينفذ SQL. ليست كل عملية في WinForms تمر عبر API؛ مثل قوائم الوثائق والأسئلة تُحمّل مباشرة من Business، بينما رفع PDF وبدء توليد الأسئلة يمران عبر HTTP.

## 2. بداية البرنامج ودخول الموظف

[Program.Main](<J:/gradeat project/DVLD Project Final/Project/DVLD/Program.cs:18>) يضبط WinForms ثم ينفذ `Application.Run(new frmLogin())`. السطر الذي يفتح `frmMain` مباشرة معلّق، فلا يمثل نقطة الدخول الحالية.

[frmLogin.btnLogin_Click](<J:/gradeat project/DVLD Project Final/Project/DVLD/Login/frmLogin.cs:28>) يقرأ الاسم وكلمة المرور بعد `Trim`، ثم يستدعي `clsUser.FindByUsernameAndPassword`. Business يستدعي `clsUserData.GetUserInfoByUsernameAndPassword` في [clsUser.cs](<J:/gradeat project/DVLD Project Final/Project/DVLD_Buisness/clsUser.cs>) و[clsUserData.cs](<J:/gradeat project/DVLD Project Final/Project/DVLD_DataAccess/clsUserData.cs>). إذا لم يوجد المستخدم تظهر رسالة فشل؛ وإذا وجد لكنه غير فعال يُمنع الدخول. عند النجاح يُحفظ الكائن في `clsGlobal.CurrentUser`، تُخفى شاشة الدخول، وتُفتح `new frmMain(this)` بواسطة `ShowDialog()`.

`frmMain.signOutToolStripMenuItem_Click` يمسح `CurrentUser`، ويعرض شاشة الدخول ويغلق الشاشة الرئيسية. تسجيل الدخول هذا خاص بموظف Desktop؛ لا يولد token للموبايل أو يثبت هوية مستخدم HTTP.

[clsGlobal.RememberUsernameAndPassword / GetStoredCredential](<J:/gradeat project/DVLD Project Final/Project/DVLD/Global Classes/clsGlobal.cs>) يكتبان ويقرآن اسم المستخدم وكلمة المرور في `data.txt` داخل مجلد العمل باستخدام فاصل `#//#`. هذا حفظ نصي كما هو، وليس تخزينًا مشفرًا. الدليل يشرح السلوك ولا ينسخ الملف أو محتواه. نوع كلمة المرور وحمايتها في قاعدة البيانات لا ينبغي استنتاجهما من اسم شاشة الدخول وحده.

## 3. نموذج الإدارة التقليدي

تستخدم عدة Business classes نمط `enMode.AddNew / Update`. `Find` يحمل سجلًا من DataAccess ويعيد كائنًا بوضع Update؛ الكائن الجديد يبدأ AddNew؛ `Save` يختار عملية INSERT أو UPDATE. قوائم الإدارة تستقبل `DataTable` ثم تربطها بـ`DataGridView` وتعرض التفاصيل في Forms أو UserControls. هذا نمط موجود في المصدر، وليس EF Core.

| المجال | الشاشات الرئيسية | كائنات Business وDataAccess |
|---|---|---|
| الأشخاص | [frmListPeople / frmAddUpdatePerson](<J:/gradeat project/DVLD Project Final/Project/DVLD/People/frmListPeople.cs>)، [بطاقة الشخص](<J:/gradeat project/DVLD Project Final/Project/DVLD/People/Controls/ctrlPersonCard.cs>) | [clsPerson](<J:/gradeat project/DVLD Project Final/Project/DVLD_Buisness/clsPerson.cs>) و[clsPersonData](<J:/gradeat project/DVLD Project Final/Project/DVLD_DataAccess/clsPersonData.cs>) |
| الموظفون | [frmListUsers](<J:/gradeat project/DVLD Project Final/Project/DVLD/User/frmListUsers.cs>)، [frmAddUpdateUser](<J:/gradeat project/DVLD Project Final/Project/DVLD/User/frmAddUpdateUser.cs>)، [frmChangePassword](<J:/gradeat project/DVLD Project Final/Project/DVLD/User/frmChangePassword.cs>) | [clsUser](<J:/gradeat project/DVLD Project Final/Project/DVLD_Buisness/clsUser.cs>) و[clsUserData](<J:/gradeat project/DVLD Project Final/Project/DVLD_DataAccess/clsUserData.cs>) |
| الطلبات المحلية | [frmListLocalDrivingLicesnseApplications](<J:/gradeat project/DVLD Project Final/Project/DVLD/Applications/Local Driving License/frmListLocalDrivingLicesnseApplications.cs>) و[frmAddUpdateLocalDrivingLicesnseApplication](<J:/gradeat project/DVLD Project Final/Project/DVLD/Applications/Local Driving License/frmAddUpdateLocalDrivingLicesnseApplication.cs>) | [clsApplication](<J:/gradeat project/DVLD Project Final/Project/DVLD_Buisness/clsApplication.cs>) و[clsLocalDrivingLicenseApplication](<J:/gradeat project/DVLD Project Final/Project/DVLD_Buisness/clsLocalDrivingLicenseApplication.cs>)، مع DataAccess المقابل |
| أنواع الطلبات والاختبارات | [frmListApplicationTypes](<J:/gradeat project/DVLD Project Final/Project/DVLD/Applications/Application Types/frmListApplicationTypes.cs>) و[frmListTestTypes](<J:/gradeat project/DVLD Project Final/Project/DVLD/Tests/Test Types/frmListTestTypes.cs>) | [clsApplicationType](<J:/gradeat project/DVLD Project Final/Project/DVLD_Buisness/clsApplicationType.cs>) و[clsTestType](<J:/gradeat project/DVLD Project Final/Project/DVLD_Buisness/clsTestType.cs>) |
| المواعيد والنتائج | [frmListTestAppointments](<J:/gradeat project/DVLD Project Final/Project/DVLD/Tests/frmListTestAppointments.cs>)، [frmScheduleTest](<J:/gradeat project/DVLD Project Final/Project/DVLD/Tests/frmScheduleTest.cs>)، [frmTakeTest](<J:/gradeat project/DVLD Project Final/Project/DVLD/Tests/frmTakeTest.cs>) | [clsTestAppointment](<J:/gradeat project/DVLD Project Final/Project/DVLD_Buisness/clsTestAppointment.cs>) و[clsTest](<J:/gradeat project/DVLD Project Final/Project/DVLD_Buisness/clsTest.cs>) |
| السائقون والرخص | [frmListDrivers](<J:/gradeat project/DVLD Project Final/Project/DVLD/Drivers/frmListDrivers.cs>) و[frmShowPersonLicenseHistory](<J:/gradeat project/DVLD Project Final/Project/DVLD/Licenses/frmShowPersonLicenseHistory.cs>) | [clsDriver](<J:/gradeat project/DVLD Project Final/Project/DVLD_Buisness/clsDriver.cs>)، [clsLicense](<J:/gradeat project/DVLD Project Final/Project/DVLD_Buisness/clsLicense.cs>)، [clsInternationalLicense](<J:/gradeat project/DVLD Project Final/Project/DVLD_Buisness/clsInternationalLicense.cs>) |

في الطلبات، `enApplicationType` يتضمن إصدار رخصة، تجديدًا، بدل فاقد، بدل تالف، فك حجز، رخصة دولية وإعادة اختبار. حالات الطلب `New=1 / Cancelled=2 / Completed=3`. `clsApplication.Cancel` و`SetComplete` يديران حالة الطلب؛ تفاصيل خدمة معينة تحفظها الطبقة أو الجدول المرتبطان بالطلب.

## 4. دورة الرخصة المحلية والخدمات التابعة

`clsLocalDrivingLicenseApplication.Save` يحفظ الطلب الأساسي ثم معلومات طلب الرخصة المحلية. `DoesPassPreviousTest` يقرر متطلبات ترتيب الاختبارات: النظري يتطلب اجتياز النظر، والشارع يتطلب اجتياز النظري. `clsTest.PassedAllTests` يعتبر النجاح في الاختبارات الثلاثة شرطًا مكتملًا في هذا النموذج. يجب التفريق بين هذه القواعد وبين إثبات أن جميع مسارات الخطأ والتزامن جُرّبت.

`clsLocalDrivingLicenseApplication.IssueLicenseForTheFirtTime` يبحث عن السائق بـ`clsDriver.FindByPersonID`، وينشئه عند الحاجة، ثم ينشئ الرخصة المرتبطة بالطلب ويكمل الطلب. الاسم المكتوب `FirtTime` هو اسم الدالة الحقيقي. شاشة الإصدار هي [frmIssueDriverLicenseFirstTime](<J:/gradeat project/DVLD Project Final/Project/DVLD/Licenses/Local Licenses/frmIssueDriverLicenseFirstTime.cs>).

في [clsLicense](<J:/gradeat project/DVLD Project Final/Project/DVLD_Buisness/clsLicense.cs>) توجد العمليات التالية:

- `RenewLicense`: ينشئ طلب تجديد، ينشئ رخصة فعالة بتاريخ جديد ومدة صلاحية فئة الرخصة، ثم يعطّل القديمة. الشاشة: [frmRenewLocalDrivingLicenseApplication](<J:/gradeat project/DVLD Project Final/Project/DVLD/Applications/Renew Local License/frmRenewLocalDrivingLicenseApplication.cs>).
- `Replace`: ينشئ طلب بدل فاقد أو تالف ورخصة بديلة، ويحافظ على تاريخ انتهاء الرخصة القديمة ثم يعطلها. الشاشة: [frmReplaceLostOrDamagedLicenseApplication](<J:/gradeat project/DVLD Project Final/Project/DVLD/Applications/ReplaceLostOrDamagedLicense/frmReplaceLostOrDamagedLicenseApplication.cs>).
- `Detain`: ينشئ سجل حجز مع الغرامة والموظف. الشاشة: [frmDetainLicenseApplication](<J:/gradeat project/DVLD Project Final/Project/DVLD/Licenses/Detain License/frmDetainLicenseApplication.cs>).
- `ReleaseDetainedLicense`: ينشئ طلب فك الحجز، ثم يحدث سجل الحجز عبر `clsDetainedLicense.ReleaseDetainedLicense`. الشاشة: [frmReleaseDetainedLicenseApplication](<J:/gradeat project/DVLD Project Final/Project/DVLD/Applications/Rlease Detained License/frmReleaseDetainedLicenseApplication.cs>).

خدمات الرخص تنفذ عدة عمليات حفظ متتابعة؛ لا يفترض الدليل أن جميع هذه الخطوات ضمن transaction واحدة. الامتحان الرسمي لديه معاملات SQL موضحة في الدليل 04، ولا يجوز تعميمها تلقائيًا على التجديد والإصدار.

الرخصة الدولية لها شاشات [frmNewInternationalLicenseApplication](<J:/gradeat project/DVLD Project Final/Project/DVLD/Applications/International License/frmNewInternationalLicenseApplication.cs>) و[frmListInternationalLicesnseApplications](<J:/gradeat project/DVLD Project Final/Project/DVLD/Applications/International License/frmListInternationalLicesnseApplications.cs>) وكائن `clsInternationalLicense`. تفاصيل قراءة الرخصة عبر HTTP موثقة في الدليل 04.

`frmTakeTest.btnSave_Click` يسجل نتيجة Pass/Fail وملاحظات وموظفًا في `clsTest.Save`. هذه شاشة تسجيل نتيجة موجودة، وليست دليلًا على وجود واجهة مرشح الامتحان الإلكتروني ضمن هذه الملفات. الـbackend الإلكتروني الذي يسحب أسئلة ويصححها موثق منفصلًا.

## 5. إدارة الوثائق

[frmAddDecoument](<J:/gradeat project/DVLD Project Final/Project/DVLD/Documents/frmAddDecoument.cs>) يستخدم:

1. `btnBrowser_Click`: اختيار PDF موجود، قراءة اسمه وحجمه وتفعيل زر الرفع.
2. `btnUpload_Click`: فتح `FileStream`، إنشاء `HttpClient` بمهلة خمس دقائق و`MultipartFormDataContent`، وإرسال الملف إلى `POST https://localhost:7077/api/knowledge-documents`.
3. [ProgressableStreamContent.SerializeToStreamAsync](<J:/gradeat project/DVLD Project Final/Project/DVLD/Documents/ProgressableStreamContent.cs>): نقل stream وإبلاغ الشاشة بعدد البايتات المنقولة؛ الشاشة تحدّث ProgressBar عبر `BeginInvoke`.
4. عند النجاح يظهر أن المعالجة ستستمر في الخلفية. وصول شريط الرفع إلى 100% لا يعني اكتمال استخراج النص أو بناء vectors.

[frmManageDocuments](<J:/gradeat project/DVLD Project Final/Project/DVLD/Documents/frmManageDocuments.cs>) يحمل SQL مباشرة عبر `_RefreshDocumentsList → clsKnowledgeDocument.GetAllDocuments`، ويعرض الاسم والحالة والحجم والمقاطع. أحداث الفلاتر تقيّد DataView، وأحداث التفعيل والتعطيل تستدعي `Activate / Deactivate`. `reprocessDocumentToolStripMenuItem_Click` يرسل POST لإعادة المعالجة بمهلة 30 دقيقة. مسار إعادة المعالجة الحالي ينتظر انتهائها بالـAPI، بخلاف رفع الوثيقة الأصلي الذي يستخدم Queue.

[clsKnowledgeDocument](<J:/gradeat project/DVLD Project Final/Project/DVLD_Buisness/clsKnowledgeDocument.cs>) يمثل metadata: الاسم الأصلي والمخزن، FilePath، الحجم، عدد الصفحات والمقاطع، الحالة، التواريخ، الخطأ والتفعيل. `GetActiveReadyDocumentIDs` هو بوابة اختيار الوثائق للبحث وشاشة التوليد. SQL لا يحفظ ملف PDF نفسه في هذه الطبقة؛ يحفظ مساره.

## 6. بنك الأسئلة والمراجعة البشرية

[frmManageQuestions](<J:/gradeat project/DVLD Project Final/Project/DVLD/QuestionBank/frmManageQuestions.cs>) يحمّل `clsQuestionBank.GetAllQuestions` داخل `_RefreshQuestionsList`، ويدعم الفلترة والعرض والتعديل والإضافة والاعتماد والرفض والتفعيل والتعطيل والحذف. الأحداث ذات الصلة هي `approveQuestionToolStripMenuItem_Click` و`rejectQuestionToolStripMenuItem_Click` و`activateQuestionToolStripMenuItem_Click` و`deactivateQuestionToolStripMenuItem_Click` و`deleteQuestionToolStripMenuItem_Click`.

[frmAddUpdateQuestion](<J:/gradeat project/DVLD Project Final/Project/DVLD/QuestionBank/frmAddUpdateQuestion.cs>) يتعامل مع كائن `clsQuestionBank`؛ `Save` يستدعي `ValidateAndNormalize`. حدود التحقق في Business: نص السؤال من 1 إلى 1000 حرف، كل خيار حتى 500 حرف، التفسير حتى 2000 حرف. MultipleChoice يحتاج أربعة خيارات وجواب A/B/C/D؛ TrueFalse يثبت الخيارين «صح/خطأ» ويقبل A/B فقط.

السؤال اليدوي يبدأ بلا `SourceDocumentID` أو صفحة أو chunk، وبحالة Draft. في التوليد الآلي يوجد مسار static إلى DataAccess؛ لا يعني وجود التحقق في `Save()` أنه يُطبق تلقائيًا على كل استدعاء static. لذلك يشرح الدليل 03 التحقق المنفذ فعليًا في مسار AI.

في [clsQuestionBank._UpdateQuestion](<J:/gradeat project/DVLD Project Final/Project/DVLD_Buisness/clsQuestionBank.cs>) و[clsQuestionBankData.UpdateQuestion](<J:/gradeat project/DVLD Project Final/Project/DVLD_DataAccess/clsQuestionBankData.cs:538>) تعديل نص السؤال أو خياراته أو إجابته يعيده إلى Draft. بيانات المصدر لا تتغير بهذا UPDATE. `Approve / Reject` يغيران حالة المراجعة، و`Activate / Deactivate` يغيران التفعيل؛ التفعيل والاعتماد محوران مختلفان.

[ctrlQuestionInfo._FillQuestionInfo / LoadQuestionInfo](<J:/gradeat project/DVLD Project Final/Project/DVLD/QuestionBank/Controls/ctrlQuestionInfo.cs>) يعرض النص والخيارات والإجابة والتفسير والوثيقة والصفحة. عرض الإجابة الصحيحة هنا جزء من شاشة الموظف؛ يختلف عن رد أسئلة الامتحان للمرشح الذي يجب ألا يسرّب الإجابة.

[clsQuestionBankData.DeleteQuestion](<J:/gradeat project/DVLD Project Final/Project/DVLD_DataAccess/clsQuestionBankData.cs:742>) يحذف فقط إذا لم يوجد السؤال في `OfficialExamQuestions`. هذه حماية للأسئلة المستخدمة بالامتحان، وليست نظام Safe Delete كاملًا لوثائق المصادر.

## 7. شاشة Generate ومسار الإشعار الحالي

الشاشة الرئيسية تفتح `new frmManageQuestions(this)`، وإدارة الأسئلة تفتح `new frmGenerateQuestions(_frmMain)` بـ`ShowDialog()`. هذا التمرير يحفظ الاتصال مع الشاشة الرئيسية لمتابعة العملية بعد إغلاق نافذة التوليد.

[frmGenerateQuestions](<J:/gradeat project/DVLD Project Final/Project/DVLD/QuestionBank/frmGenerateQuestions.cs>) يحتوي:

- `_LoadReadyDocuments`: يحمل معرفات الوثائق الفعالة والجاهزة من SQL، ثم أسماءها؛ `cmbDocuments.ValueMember = DocumentID`.
- `_UpdateTotalQuestions`: يجمع `nudMultipleChoice` و`nudTrueFalse` ويعرض `lblTotalQuestions`، ويوقف زر Generate عند مجموع أكبر من 100 أو صفر أو عدم وجود وثائق.
- `frmGenerateQuestions_Load`: يثبت الحد الأعلى لكل NumericUpDown على 100، ويحمل الوثائق ويحدث الإجمالي.
- `btnGenerate_Click`: يحوّل documentID والعددين إلى JSON بـ`System.Text.Json.JsonSerializer.Serialize`، ويضع UTF-8 و`application/json` داخل `StringContent`، ويرسل POST بمهلة 30 ثانية.
- بعد Deserialize مع `PropertyNameCaseInsensitive=true` يتأكد من Job ID، يستدعي `_frmMain.TrackQuestionGenerationJob` إن وجد، ويعرض بدء العملية ويغلق النافذة.

داخل [frmMain.TrackQuestionGenerationJob / _QuestionGenerationTimer_Tick](<J:/gradeat project/DVLD Project Final/Project/DVLD/frmMain.cs:50>) يُحفظ Job ID واحد، ويبدأ WinForms Timer مدته 5000ms. كل Tick يوقف Timer قبل الطلب، ثم يستدعي GET حالة العملية. Pending/Processing يعيدان تشغيله؛ Completed يعرض MessageBox ويفتح إدارة الأسئلة الخاصة بالوثيقة والعدد.

هذا إشعار MessageBox داخل التطبيق، وليس NotifyIcon أو Toast أو سجل إشعارات. الكود الحالي لا يعرض ErrorMessage عند Failed. كما أن `_RefreshQuestionsList` عند عرض النتيجة يختار أحدث أسئلة Draft للوثيقة حسب العدد، ولا يربطها مباشرة بالـJob.

## 8. ما يجب وصفه بدقة عند العرض

- الدخول الحالي دخول موظف Desktop؛ صلاحيات الأدوار ومصادقة HTTP ليست مستنتجة من نجاح هذا الدخول.
- التوليد يبدأ من شاشة موظف ثم ينتقل لخادم API؛ إغلاق الشاشة لا يلغي Job تلقائيًا.
- المتابعة الحالية لطلب واحد في ذاكرة التطبيق؛ لا توجد قائمة جميع jobs أو استرجاع متابعة بعد إغلاق التطبيق.
- زر التوليد الحالي بلا catch حول HTTP وبلا فحص status code قبل Deserialize وبلا تعطيل أثناء الإرسال؛ هذه حدود موثقة ولم تُصلح هنا.
- Frontend الموبايل وإعادة التصميم التي سيعملها الفريق ليست ملفات موجودة في النسخة المفحوصة؛ لا يصفها الدليل كمنجزة.

تفصيل معالجة PDF والشات في الدليل 02، وتوليد الأسئلة ومصدرها في 03، وتتبع APIs والامتحان في 04.
