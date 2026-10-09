# دليل الـAPI والامتحان الرسمي في مشروع DVLD

هذا الفصل يشرح ملفات المشروع الموجودة محليًا بتاريخ 6 أكتوبر 2026، بما فيها محتوى الملفات غير المرفوع إلى GitHub. الشرح مبني على قراءة الكود؛ لم يُشغّل التطبيق أو قاعدة البيانات ولم يُنفّذ ملف SQL. لا تُعامل الأمثلة التالية على أنها نتائج مأخوذة من سجلات حقيقية. الامتحان الرسمي مخصص للكمبيوتر في مركز الامتحان حسب نطاق المشروع الذي حدده صاحب المشروع، وليس امتحانًا رسميًا على الموبايل.

## 1. دور الـAPI وعلاقته بالطبقات

مشروع [DVLD.Api.csproj](<J:/gradeat project/DVLD Project Final/Project/DVLD.Api/DVLD.Api.csproj>) هو ASP.NET Core يستهدف `net10.0`، ويرتبط بمشروعي `DVLD_Buisness` و`DVLD.AI`. أسماء المسارات `DVLD_Buisness` و`DVLD_DataAccess` هي الأسماء الموجودة فعلًا، بما فيها التهجئة الحالية.

في وظائف الرخص والطلبات والمواعيد، التسلسل المعتاد هو:

```text
طلب HTTP
  → Controller داخل DVLD.Api/Controllers
  → Class ودالة داخل DVLD_Buisness
  → Class ودالة داخل DVLD_DataAccess
  → SqlConnection / SqlCommand / SqlDataReader
  → جدول أو مجموعة جداول SQL Server
  → DataTable أو كائن Business
  → حقول محددة في رد HTTP
```

الـControllers لا ترجع كائنات Business كاملة في هذه المسارات، بل تبني كائنات تحتوي حقول الرد المطلوبة. هذه نقطة مهمة لأن كائن الرخصة أو الطلب قد يحمل داخليًا معلومات إضافية مثل المستخدم الذي أنشأ السجل.

## 2. إعداد الخادم والاتصال بقاعدة البيانات

في [Program.cs](<J:/gradeat project/DVLD Project Final/Project/DVLD.Api/Program.cs>) توجد:

- `AddControllers()` لتفعيل Controllers وربط الطلبات والردود.
- تسجيل Queue وBackground Worker لمعالجة الوثائق وQueue وWorker لتوليد الأسئلة؛ تفاصيلهما في فصل الـAI.
- `AddOpenApi()`، ثم `MapOpenApi()` في بيئة Development.
- `AddProblemDetails()` و`UseExceptionHandler()` لمعالجة الاستثناءات التي لم تُعالج داخل المسار.
- `UseHttpsRedirection()` ثم `UseAuthorization()` ثم `MapControllers()`.

عناوين التطوير في [launchSettings.json](<J:/gradeat project/DVLD Project Final/Project/DVLD.Api/Properties/launchSettings.json>) هي `https://localhost:7077` و`http://localhost:5277` في ملف التشغيل HTTPS. هذا إعداد تشغيل محلي، ولا يثبت أن الخدمة تعمل الآن أو أن هاتفًا خارجيًا يستطيع الوصول إليها.

مصدر الاتصال الوحيد الظاهر في طبقة البيانات هو [clsDataAccessSettings.ConnectionString](<J:/gradeat project/DVLD Project Final/Project/DVLD_DataAccess/clsDataAccessSettings.cs>):

1. تقرأ `DVLD_CONNECTION_STRING` من بيئة العملية.
2. إن كانت فارغة، تقرؤها من متغيرات بيئة المستخدم `EnvironmentVariableTarget.User`.
3. إن كانت غير مهيأة، ترمي `InvalidOperationException` برسالة تفيد بأن متغير الاتصال غير مضبوط.

لا يوجد في هذا الكود اختيار تلقائي لقاعدة بيانات بديلة عند غياب المتغير. لم تُقرأ قيمة المتغير أو بيانات دخول قاعدة البيانات ضمن إعداد هذا الدليل.

الـControllers تستعمل أسماء خصائص C# مثل `LicenseID` و`ExamAttemptID`. لا توجد في `Program.cs` تهيئة JSON مخصصة؛ التسلسل الافتراضي في ASP.NET Core يستخدم أسماء JSON بصيغة camelCase، مثل `licenseID` و`examAttemptID`. يورد هذا الفصل أسماء خصائص C# كما هي في الكود لتسهيل مطابقتها.

## 3. خريطة المسارات الأساسية

| HTTP والمسار | الدالة في Controller | الوظيفة |
|---|---|---|
| `GET /api/people/{personId}/licenses` | `LicensesController.GetPersonLicenses` | الرخص المحلية الفعالة وغير المنتهية لشخص |
| `GET /api/licenses/{licenseId}` | `LicensesController.GetLicenseById` | تفاصيل رخصة محلية محددة |
| `GET /api/people/{personId}/international-licenses` | `InternationalLicensesController.GetPersonInternationalLicenses` | الرخص الدولية الفعالة وغير المنتهية لشخص |
| `GET /api/international-licenses/{internationalLicenseId}` | `InternationalLicensesController.GetInternationalLicenseById` | تفاصيل رخصة دولية |
| `GET /api/people/{personId}/applications` | `ApplicationsController.GetPersonApplications` | طلبات شخص وتفاصيلها الأساسية |
| `GET /api/applications/{applicationId}` | `ApplicationsController.GetApplicationById` | طلب مع تفاصيل تختلف بحسب نوعه |
| `GET /api/people/{personId}/test-appointments` | `TestAppointmentsController.GetPersonTestAppointments` | مواعيد الاختبارات ونتائجها إن وجدت |
| `GET /api/test-appointments/{testAppointmentId}` | `TestAppointmentsController.GetTestAppointmentById` | موعد واحد مع نتيجة الاختبار إن وجدت |
| `GET /api/people` | `PeopleController.GetAllPeople` | قائمة الأشخاص |
| `GET /api/people/{id}` | `PeopleController.GetPersonById` | شخص بالمعرّف |
| `GET /api/people/national/{nationalNo}` | `PeopleController.GetPersonByNationalNo` | شخص بالرقم الوطني؛ بحث وليس تسجيل دخول |
| `GET /api/countries` | `CountriesController.GetAllCountries` | الدول |
| `GET /api/countries/{id}` | `CountriesController.GetCountryById` | دولة واحدة |
| `GET /api/health` | `HealthController.Get` | رد بسيط بأن خدمة API تستجيب |
| `POST /api/official-exams/start` | `OfficialExamsController.Start` | بدء محاولة امتحان رسمي |
| `PUT /api/official-exams/{examAttemptID}/answers/{examQuestionID}` | `OfficialExamsController.SaveAnswer` | حفظ إجابة واحدة أو مسحها |
| `POST /api/official-exams/{examAttemptID}/submit` | `OfficialExamsController.Submit` | تصحيح المحاولة وإغلاقها وحفظ نتيجتها |

ملفات Controllers المشار إليها: [LicensesController.cs](<J:/gradeat project/DVLD Project Final/Project/DVLD.Api/Controllers/LicensesController.cs>)، [InternationalLicensesController.cs](<J:/gradeat project/DVLD Project Final/Project/DVLD.Api/Controllers/InternationalLicensesController.cs>)، [ApplicationsController.cs](<J:/gradeat project/DVLD Project Final/Project/DVLD.Api/Controllers/ApplicationsController.cs>)، [TestAppointmentsController.cs](<J:/gradeat project/DVLD Project Final/Project/DVLD.Api/Controllers/TestAppointmentsController.cs>)، [PeopleController.cs](<J:/gradeat project/DVLD Project Final/Project/DVLD.Api/Controllers/PeopleController.cs>)، [CountriesController.cs](<J:/gradeat project/DVLD Project Final/Project/DVLD.Api/Controllers/CountriesController.cs>)، [HealthController.cs](<J:/gradeat project/DVLD Project Final/Project/DVLD.Api/Controllers/HealthController.cs>)، [OfficialExamsController.cs](<J:/gradeat project/DVLD Project Final/Project/DVLD.Api/Controllers/OfficialExamsController.cs>).

## 4. الرخص المحلية: من المسار إلى الجدول

### 4.1 قائمة رخص الشخص

`LicensesController.GetPersonLicenses(personId)` يبدأ بفحص وجود الشخص باستخدام `clsPerson.Find(personId)`، ثم يبحث عن سجل السائق عبر `clsDriver.FindByPersonID(personId)`.

- إذا لم يوجد الشخص: `404 Not Found`.
- إذا وجد الشخص ولم يوجد سائق له: `200 OK` مع قائمة فارغة.
- إذا وجد السائق: تقرأ الرخص وتجهز قائمة الرد.

سلسلة قراءة الرخص هي:

```text
clsDriver.GetLicenses(driver.DriverID)
  → clsLicense.GetDriverLicenses(DriverID)
  → clsLicenseData.GetDriverLicenses(DriverID)
  → Licenses INNER JOIN LicenseClasses
     WHERE DriverID = @DriverID
```

المراجع: [clsDriver.cs](<J:/gradeat project/DVLD Project Final/Project/DVLD_Buisness/clsDriver.cs>)، [clsLicense.cs في Business](<J:/gradeat project/DVLD Project Final/Project/DVLD_Buisness/clsLicense.cs>)، [clsLicense.cs في DataAccess](<J:/gradeat project/DVLD Project Final/Project/DVLD_DataAccess/clsLicense.cs>).

SQL يُرتّب حسب النشاط ثم تاريخ الانتهاء. بعدها يرشّح Controller النتائج مرة أخرى بحيث يستبعد `IsActive = false` أو `ExpirationDate < DateTime.Now`. لذلك هذا المسار لا يمثل سجل الرخص الكامل؛ يمثل الرخص الفعالة غير المنتهية وفق وقت عملية API.

لكل رخصة باقية يستدعي `clsDetainedLicense.IsLicenseDetained(licenseID)`، ومنها `clsDetainedLicenseData.IsLicenseDetained` لقراءة حالة الحجز من `DetainedLicenses`.

الرد يحتوي `LicenseID`, `ApplicationID`, `ClassName`, `IssueDate`, `ExpirationDate`, `IsActive`, `IsDetained`. حقول الحجز تُحسب باستعلام إضافي لكل رخصة، وليست جزءًا من استعلام قائمة الرخص الأصلي.

### 4.2 تفاصيل رخصة محلية

`GetLicenseById(licenseId)` يستدعي `clsLicense.Find(licenseId)`، التي تقرأ السجل من `clsLicenseData.GetLicenseInfoByID` باستخدام `LicenseID` في جدول `Licenses`. إن لم يوجد السجل يرجع `404`.

عند بناء كائن Business، تُحمّل معلومات السائق وفئة الرخصة وحالة الحجز والمستخدم المرتبط بها. `clsDriver` يحمّل `clsPerson`، و`clsPerson` يحمل معلومات الدولة. المسارات المساعدة الأساسية هي [clsPerson.cs](<J:/gradeat project/DVLD Project Final/Project/DVLD_Buisness/clsPerson.cs>) و[clsPersonData.cs](<J:/gradeat project/DVLD Project Final/Project/DVLD_DataAccess/clsPersonData.cs>)، و[clsLicenseClass.cs](<J:/gradeat project/DVLD Project Final/Project/DVLD_Buisness/clsLicenseClass.cs>) و[LicenseClass.cs](<J:/gradeat project/DVLD Project Final/Project/DVLD_DataAccess/LicenseClass.cs>).

رد التفاصيل يحتوي معرفات الرخصة والطلب والشخص، الاسم والرقم الوطني، فئة الرخصة ووصفها، تواريخها، `Notes`, `PaidFees`, `IsActive`, `IsExpired`, `IssueReason`, `IssueReasonText`, `IsDetained`. `IsExpired` محسوبة عبر `clsLicense.IsLicenseExpired()`.

مسار التفاصيل لا يستبعد رخصة لأنها منتهية أو غير فعالة، بل يوضح حالتها في الرد. وجود NationalNo أو FullName هنا لا يعني أن API تحقق هوية من يطلب السجل.

## 5. الرخص الدولية

### 5.1 قائمة رخص الشخص

`GetPersonInternationalLicenses(personId)` يفحص الشخص ثم السائق بنفس سلوك `404` والقائمة الفارغة السابق. سلسلة القراءة هي:

```text
clsDriver.GetInternationalLicenses(driver.DriverID)
  → clsInternationalLicense.GetDriverInternationalLicenses(DriverID)
  → clsInternationalLicenseData.GetDriverInternationalLicenses(DriverID)
  → InternationalLicenses WHERE DriverID = @DriverID
```

المراجع: [clsInternationalLicense.cs في Business](<J:/gradeat project/DVLD Project Final/Project/DVLD_Buisness/clsInternationalLicense.cs>) و[clsInternationalLicense.cs في DataAccess](<J:/gradeat project/DVLD Project Final/Project/DVLD_DataAccess/clsInternationalLicense.cs>).

تُرتّب النتائج حسب انتهاء الرخصة، ويستبعد Controller غير الفعال والمنتهي. الرد يحتوي `InternationalLicenseID`, `ApplicationID`, `IssuedUsingLocalLicenseID`, `IssueDate`, `ExpirationDate`, `IsActive`.

### 5.2 تفاصيل رخصة دولية

`GetInternationalLicenseById` يستدعي `clsInternationalLicense.Find`، ثم `clsInternationalLicenseData.GetInternationalLicenseInfoByID`. كائن الرخصة الدولية يرث من `clsApplication`؛ لذلك بعد قراءة الرخصة تُقرأ معلومات الطلب الأساسي باستخدام `clsApplication.FindBaseApplication(ApplicationID)`، ومنها `clsApplicationData.GetApplicationInfoByID` في جدول `Applications`.

الرد يحتوي معلومات الرخصة والسائق والشخص والرقم الوطني والرخصة المحلية التي أُصدرت باستخدامها، التواريخ والنشاط، `IsExpired`, `IsCurrentlyValid`, ومعلومات الطلب مثل `ApplicationDate`, `ApplicationStatus`, `PaidFees`.

`IsCurrentlyValid` تعني `IsActive && !IsExpired`. فحص الانتهاء هنا يعتمد أيضًا على `DateTime.Now`. الكود يفترض في `clsInternationalLicense.Find` أن الطلب الأساسي موجود؛ لم يُفحص قيد الربط الحقيقي في قاعدة البيانات لإثبات هذا الافتراض على كل البيانات.

## 6. الطلبات وأنواع التفاصيل

### 6.1 قائمة الطلبات

`ApplicationsController.GetPersonApplications(personId)` يفحص الشخص ثم:

```text
clsApplication.GetPersonApplications(PersonID)
  → clsApplicationData.GetPersonApplications(PersonID)
  → Applications A
     LEFT JOIN LocalDrivingLicenseApplications LDLA
     LEFT JOIN LicenseClasses LC
     WHERE A.ApplicantPersonID = @PersonID
     ORDER BY A.ApplicationDate DESC
```

المراجع: [clsApplication.cs في Business](<J:/gradeat project/DVLD Project Final/Project/DVLD_Buisness/clsApplication.cs>) و[clsApplication.cs في DataAccess](<J:/gradeat project/DVLD Project Final/Project/DVLD_DataAccess/clsApplication.cs>).

`LEFT JOIN` يسمح بظهور الطلبات التي لا يوجد لها سجل `LocalDrivingLicenseApplications`. الرد يحتوي `ApplicationID`, `ApplicantPersonID`, `ApplicationDate`, `ApplicationTypeID`, `ApplicationTypeName`, `ApplicationStatus`, `StatusText`, `LastStatusDate`, `PaidFees`, والحقول الاختيارية `LocalDrivingLicenseApplicationID`, `LicenseClassID`, `ClassName`. تُحوّل قيم `DBNull` للحقول الاختيارية إلى `null`.

الحالات في `clsApplication.enApplicationStatus` و`GetStatusText` هي `1 = New`, `2 = Cancelled`, `3 = Completed`، والقيمة غير المعروفة تعرض `Unknown`.

### 6.2 تفاصيل طلب واحد

`GetApplicationById(applicationId)` يقرأ الطلب الأساسي باستخدام `clsApplication.FindBaseApplication` ثم يختار المسار حسب `ApplicationTypeID`. إذا لم يوجد الطلب الأساسي يرجع `404`.

| النوع في `clsApplication.enApplicationType` | السلسلة الأساسية | محتوى `Details` |
|---|---|---|
| `1 NewDrivingLicense` | `clsLocalDrivingLicenseApplication.FindByApplicationID` → `clsLocalDrivingLicenseApplicationData.GetLocalDrivingLicenseApplicationInfoByApplicationID` → `LocalDrivingLicenseApplications`، ثم فئة الرخصة | معرف الطلب المحلي، معرف الفئة، اسمها |
| `2 RenewDrivingLicense` | `clsLicense.GetLicenseIDByApplicationID` → `clsLicenseData.GetLicenseIDByApplicationID` → `Licenses`، ثم `clsLicense.Find` | الرخصة والفئة والتواريخ والنشاط وسبب الإصدار |
| `3 ReplaceLostDrivingLicense` | سلسلة قراءة الرخصة نفسها | بيانات رخصة البديل |
| `4 ReplaceDamagedDrivingLicense` | سلسلة قراءة الرخصة نفسها | بيانات رخصة البديل |
| `5 ReleaseDetainedDrivingLicsense` | `clsDetainedLicense.GetDetainIDByReleaseApplicationID` → `clsDetainedLicenseData.GetDetainIDByReleaseApplicationID` → `DetainedLicenses`، ثم `clsDetainedLicense.Find` | الحجز والرخصة وتاريخ الحجز والغرامة والتحرير |
| `6 NewInternationalLicense` | `clsInternationalLicense.GetInternationalLicenseIDByApplicationID` → DataAccess بنفس الاسم → `InternationalLicenses`، ثم `Find` | الرخصة الدولية والسائق والرخصة المحلية والتواريخ والصلاحية |
| `7 RetakeTest` | `clsTestAppointment.GetTestAppointmentIDByRetakeApplicationID` → DataAccess بنفس الاسم → `TestAppointments`، ثم `Find` | الموعد ونوع الاختبار والطلب المحلي والرسوم والقفل و`TestID` |

المراجع الإضافية: [clsLocalDrivingLicenseApplication.cs](<J:/gradeat project/DVLD Project Final/Project/DVLD_Buisness/clsLocalDrivingLicenseApplication.cs>)، [clsLocalDrivingLicenseApplicationData.cs](<J:/gradeat project/DVLD Project Final/Project/DVLD_DataAccess/clsLocalDrivingLicenseApplicationData.cs>)، [clsDetainedLicense.cs في Business](<J:/gradeat project/DVLD Project Final/Project/DVLD_Buisness/clsDetainedLicense.cs>)، [clsDetainedLicense.cs في DataAccess](<J:/gradeat project/DVLD Project Final/Project/DVLD_DataAccess/clsDetainedLicense.cs>).

كل الفروع المذكورة ترجع الحقول الأساسية للطلب، ثم `Details`. قد تكون `Details = null` إذا لم يوجد السجل المرتبط؛ طلب التجديد مثلًا لا يضمن وحده وجود رخصة مُصدرة. الأنواع الأخرى ترجع الحقول الأساسية فقط. تفاصيل إعادة الاختبار ترجع `TestID` كما هو من Business، وقد يكون `-1` بدل `null` عند غياب الاختبار.

## 7. مواعيد الاختبارات والنتائج

### 7.1 قائمة المواعيد

`GetPersonTestAppointments(personId)` يفحص الشخص ثم يستدعي `clsTestAppointment.GetPersonTestAppointments`، ومنها `clsTestAppointmentData.GetPersonTestAppointments` في [clsTestAppointment.cs في DataAccess](<J:/gradeat project/DVLD Project Final/Project/DVLD_DataAccess/clsTestAppointment.cs>).

SQL يقرأ `TestAppointments` مع `LocalDrivingLicenseApplications`, `Applications`, `TestTypes`, `LicenseClasses`، ويضيف `LEFT JOIN Tests`. التصفية `A.ApplicantPersonID = @PersonID`، والترتيب من الأحدث حسب `AppointmentDate`.

الرد يحتوي معرف الموعد، نوع الاختبار وعنوانه، معرف الطلب المحلي والأساسي وفئة الرخصة واسمها، تاريخ الموعد والرسوم والقفل، وحقول `RetakeTestApplicationID`, `TestID`, `TestResult`, `Notes`. حقول النتيجة التي لا يوجد لها سجل في `Tests` تُرجع `null`، وليس رسوبًا تلقائيًا.

### 7.2 موعد واحد

`GetTestAppointmentById` يقرأ `clsTestAppointment.Find`، ومنها `clsTestAppointmentData.GetTestAppointmentInfoByID`. ثم خاصية `TestID` تستدعي `_GetTestID`، ومنها `clsTestAppointmentData.GetTestID` لقراءة جدول `Tests`.

إذا كان `TestID = -1` تكون `Test = null`. وإلا تستدعي `clsTest.Find`، ومنها `clsTestData.GetTestInfoByID`، وتبني:

```text
Test = { TestID, TestResult, ResultText: Passed/Failed, Notes }
```

المراجع: [clsTestAppointment.cs في Business](<J:/gradeat project/DVLD Project Final/Project/DVLD_Buisness/clsTestAppointment.cs>)، [clsTest.cs في Business](<J:/gradeat project/DVLD Project Final/Project/DVLD_Buisness/clsTest.cs>)، [clsTest.cs في DataAccess](<J:/gradeat project/DVLD Project Final/Project/DVLD_DataAccess/clsTest.cs>).

غياب الموعد نفسه يرجع `404`. وجود موعد بلا اختبار يرجع `200` وتفاصيل الموعد مع `Test = null`. `TestTypeName` في التفاصيل هو اسم Enum، بينما القائمة تستخدم `TestTypeTitle` من الجدول.

## 8. People وCountries وHealth

`PeopleController.GetAllPeople` يستدعي `clsPerson.GetAllPeople` ثم `clsPersonData.GetAllPeople`. تعرض القائمة الهوية والاسم وتاريخ الميلاد والجنس والعنوان والهاتف والبريد والدولة ومسار الصورة. مسارا البحث يستدعيان overloads `clsPerson.Find(int)` و`clsPerson.Find(string)`، ومنهما `GetPersonInfoByID` و`GetPersonInfoByNationalNo`. البحث بالرقم الوطني يقرأ شخصًا ولا ينشئ جلسة دخول أو Token.

`CountriesController` يستخدم `clsCountry.GetAllCountries` أو `clsCountry.Find(id)`، ثم `clsCountryData.GetAllCountries` أو `GetCountryInfoByID` في [clsCountry.cs](<J:/gradeat project/DVLD Project Final/Project/DVLD_Buisness/clsCountry.cs>) و[clsCountryData.cs](<J:/gradeat project/DVLD Project Final/Project/DVLD_DataAccess/clsCountryData.cs>). الرد يحتوي `CountryID` و`CountryName`.

`HealthController.Get()` يرجع `{ service: "DVLD.Api", status: "Running" }`. لا يقرأ SQL Server أو Qdrant أو Ollama؛ نجاحه وحده لا يثبت أن بقية الخدمات جاهزة.

## 9. فكرة الامتحان الرسمي وعلاقته بالـAI

الـAI يولّد مرشحات أسئلة ويحفظها في `QuestionBank` للمراجعة. الامتحان الرسمي لا يستدعي نموذج AI أثناء الاختبار لتوليد السؤال أو حساب صحة الإجابة؛ يستخدم أسئلة محفوظة ونشطة وحالتها `Approved`، ثم يصحح الاختيارات بمقارنتها بالإجابة الصحيحة المجمدة في قاعدة البيانات.

هذا يفصل التوليد والمراجعة عن تنفيذ الاختبار: تغير الوثيقة أو نموذج AI أو نص السؤال لاحقًا لا يغيّر نص محاولة بدأت بالفعل، لأن الامتحان يحفظ snapshot.

الخريطة:

```text
QuestionBank: سؤال Approved وActive
  → StartExam: اختيار عشوائي ونسخ الأسئلة
  → OfficialExamAttempts: السياسة والوقت وحالة المحاولة
  → OfficialExamQuestions: نسخة الأسئلة والإجابات الصحيحة الداخلية
  → SaveAnswer: اختيارات الممتحن المحفوظة
  → SubmitExam: مقارنة + درجة + Tests + قفل الموعد
```

## 10. Precheck: فحص مبدئي وليس دخولًا أو بدءًا

ملف [clsOfficialExam_Precheck.cs](<J:/gradeat project/DVLD Project Final/Project/DVLD_Buisness/clsOfficialExam_Precheck.cs>) يحتوي class باسم `clsOfficialExam`، وليس class مطابقًا لاسم الملف. دواله:

- `GetWrittenAppointments(string nationalNo)` يعيد قائمة مواعيد النظري مع تقييم الأهلية.
- `CheckAppointment(int testAppointmentID, string nationalNo)` يفحص موعدًا محددًا يخص الشخص.
- `Evaluate(OfficialExamAppointmentInfo appointment)` يحوّل حقائق الموعد إلى حالة.

طبقة البيانات في [clsOfficialExamData.cs](<J:/gradeat project/DVLD Project Final/Project/DVLD_DataAccess/clsOfficialExamData.cs>) توفر `GetWrittenAppointmentsByNationalNo` و`GetCandidateAppointment`. تقرأ `TestAppointments → LocalDrivingLicenseApplications → Applications → People`، وتستعمل `EXISTS` لمعرفة وجود Test سابق أو OfficialAttempt سابق ونجاح اختبار النظر `TestTypeID = 1`.

الحالات الممكنة في `enPrecheckStatus`:

| القيمة | الحالة | معناها |
|---|---|---|
| 0 | `EligibleForEmployeeVerification` | الشروط المبدئية متحققة، وتبقى مراجعة الموظف |
| 1 | `AppointmentNotFound` | الموعد غير موجود أو لا يطابق الشخص |
| 2 | `NotWrittenTest` | الاختبار ليس النظري `TestTypeID = 2` |
| 3 | `ApplicationNotActive` | حالة الطلب ليست `New = 1` |
| 4 | `AppointmentLocked` | الموعد مقفل |
| 5 | `TestAlreadyRecorded` | له نتيجة في `Tests` |
| 6 | `OfficialAttemptAlreadyExists` | له محاولة رسمية سابقة |
| 7 | `VisionTestNotPassed` | لا يوجد نجاح نظر لنفس الطلب المحلي |

`PrecheckResult` يحتوي `TestAppointmentID`, `LocalDrivingLicenseApplicationID`, `AppointmentDate`, `Status`, وخاصية مشتقة `IsEligibleForEmployeeVerification`.

المؤكد من الكود: Precheck لا يبدأ امتحانًا ولا يصادق على شخص أو موظف، ولا يفحص نافذة وقت البداية. لا يوجد endpoint لـPrecheck في `OfficialExamsController`، ولم يظهر استدعاء لدالتيه من شاشة WinForms أو Controller ضمن ملفات C# التي فُحصت. توفر الدالة لا يثبت وجود شاشة موظف موصولة بها.

## 11. بدء الامتحان الرسمي

### 11.1 سياسة الخادم

`OfficialExamsController.Start` يستقبل `StartOfficialExamRequest` بحقل `NationalNo` فقط. قيمة فارغة أو request فارغ ترجع `400`.

السياسة تُقرأ من [appsettings.json](<J:/gradeat project/DVLD Project Final/Project/DVLD.Api/appsettings.json>)، والقيم الحالية في الملف:

| المفتاح | القيمة |
|---|---|
| `OfficialExam:QuestionCount` | 5 أسئلة |
| `OfficialExam:RequiredCorrectAnswers` | 4 إجابات صحيحة للنجاح |
| `OfficialExam:DurationSeconds` | 900 ثانية = 15 دقيقة |
| `OfficialExam:StartEarlyMinutes` | يسمح بالبدء قبل الموعد بـ30 دقيقة |
| `OfficialExam:StartLateMinutes` | يسمح بالبدء بعد الموعد بـ30 دقيقة |

هذه قيم الملف المحلي، وليست إثباتًا للقيم الفعلية وقت التشغيل؛ إعدادات ASP.NET Core قد تُستبدل بمصدر إعداد آخر أو متغيرات بيئة. حد الأسئلة المقبول هنا من 1 إلى 200، وحد النجاح بين 1 وعدد الأسئلة، والمدة من 1 إلى 86400 ثانية، ونافذة البدء من 0 إلى 1440 دقيقة لكل جانب. السياسة غير الموجودة أو غير الصالحة ترجع `503`.

عدد الأسئلة ومدة الامتحان وحد النجاح لا تؤخذ من JSON الممتحن. مثال شكل الطلب:

```json
{
  "nationalNo": "DEMO-NATIONAL-NO"
}
```

### 11.2 اختيار الموعد وإعادة الفحص

`Start` يستدعي `clsOfficialExamStart.StartByNationalNo` في [clsOfficialExamStart.cs](<J:/gradeat project/DVLD Project Final/Project/DVLD_Buisness/clsOfficialExamStart.cs>). تقرأ الدالة مواعيد النظري، وتستبعد غير المؤهل أو خارج نافذة البداية وفق `DateTime.Now` في عملية API، ثم تختار أقدم موعد صالح من المواعيد المتاحة.

بعد الاختيار تستدعي `StartForVerifiedCandidate`, التي تمرر الموعد والرقم الوطني والسياسة إلى `clsOfficialExamStartData.StartExam` في [clsOfficialExamStartData.cs](<J:/gradeat project/DVLD Project Final/Project/DVLD_DataAccess/clsOfficialExamStartData.cs>). اسم `ForVerifiedCandidate` وتعليقات الكود تعبر عن أن الهوية يفترض أن يراجعها موظف في مركز الامتحان؛ لا توجد في هذه الدالة عملية تحقق هوية أو مصادقة موظف تنفذ ذلك تلقائيًا.

### 11.3 المعاملة الذرية في SQL

`StartExam` يستخدم `SqlTransaction` بمستوى `IsolationLevel.Serializable`. داخلها:

1. يقفل الموعد أثناء القراءة بـ`UPDLOCK, HOLDLOCK`، ويطابق الرقم الوطني بصاحب الطلب.
2. يعيد التأكد من النظري، نشاط الطلب، عدم القفل، عدم وجود نتيجة أو محاولة رسمية، ونجاح النظر.
3. يعيد فحص نافذة الموعد باستخدام `SYSDATETIME()` من SQL Server. هذا هو فحص الوقت النهائي، حتى لو مر فحص Business السابق.
4. ينشئ `OfficialExamAttempts` بحالة `InProgress`، ويجمّد العدد وحد النجاح والمدة.
5. ينشئ `StartedAtUtc` باستخدام `SYSUTCDATETIME()` و`DeadlineAtUtc` بإضافة المدة إليه.
6. يختار أسئلة من `QuestionBank` باستخدام `TOP (@QuestionCount)` و`ORDER BY NEWID()`، بشرط `ReviewStatus = Approved` و`ISNULL(IsActive, 1) = 1` ومحتوى وخيارات صالحة.
7. ينسخ الأسئلة إلى `OfficialExamQuestions` مع نص السؤال والاختيارات والإجابة الصحيحة والتفسير ومعرف الوثيقة والصفحة. ترتيب الأسئلة المجمدة يُرقّم وفق `QuestionID` للأسئلة المختارة؛ العشوائية الأساسية هنا في اختيار مجموعة الأسئلة.
8. إن كان العدد المنسوخ أقل من المطلوب، يرمي خطأ، وترجع المعاملة كلها `Rollback`، فلا تبقى محاولة ناقصة.
9. يقرأ حقول عرض آمنة، ثم ينفذ `Commit`.

أهلية TrueFalse تشترط الإجابة `A` أو `B`. MultipleChoice يشترط وجود الخيارات الأربعة وإجابة ضمن `A/B/C/D`. لا يوجد في استعلام الاختيار الظاهر تقسيم مفروض إلى عدد ثابت من النوعين أو فلتر بحسب فئة الرخصة؛ عدد الأسئلة والسياسة هما ما يحدد الاختبار الحالي.

### 11.4 الرد وحماية الإجابة الصحيحة

`StartResult` يحتوي `ExamAttemptID`, `TestAppointmentID`, `StartedAtUtc`, `DeadlineAtUtc`, و`Questions`. كل `CandidateQuestion` يحتوي `ExamQuestionID`, `QuestionNumber`, `QuestionType`, `QuestionText`, `OptionA`, `OptionB`, `OptionC`, `OptionD` فقط.

`CorrectOption`, `Explanation`, `IsCorrect` غير موجودة في DTO المرسل للممتحن، وغير موجودة في استعلام `safeQuestionsSql`. تُحفظ الإجابة الصحيحة في SQL لاستخدامها عند التصحيح. الحماية هنا مؤكدة لمسار بدء الامتحان؛ وجود APIs إدارية للأسئلة دون مصادقة يبقى موضوعًا منفصلًا عن DTO الامتحان.

Business يضع `DateTimeKind.Utc` على أوقات البداية والنهاية لأن `datetime2` لا يحمل معلومات المنطقة الزمنية رغم أن مصدر القيم UTC. وقت الموعد القديم محلي، لذا ينبغي أن تتوافق منطقة عملية API وSQL Server مع تعريف مواعيد المركز؛ إعداد المنطقة الفعلي لم يُفحص.

أخطاء الأهلية أو نقص الأسئلة ترجع `409 Conflict`، وأخطاء المدخلات `400`. أخطاء SQL غير المعالجة تمر إلى معالج الأخطاء المركزي.

## 12. حفظ إجابة واحدة

المسار:

```text
OfficialExamsController.SaveAnswer
  → clsOfficialExamSession.SaveAnswer
  → clsOfficialExamSessionData.SaveAnswer
```

المراجع: [clsOfficialExamSession.cs](<J:/gradeat project/DVLD Project Final/Project/DVLD_Buisness/clsOfficialExamSession.cs>) و[clsOfficialExamSessionData.cs](<J:/gradeat project/DVLD Project Final/Project/DVLD_DataAccess/clsOfficialExamSessionData.cs>).

جسم الطلب مثالًا:

```json
{
  "selectedOption": "A"
}
```

و`{ "selectedOption": null }` يمسح الإجابة. طبقة البيانات تزيل المسافات وتحول الحرف إلى uppercase، وتقبل `A/B/C/D` أو null. القيم الفارغة أو المسافات تُعامل كمسح أيضًا.

تفتح الدالة معاملة Serializable وتقرأ المحاولة بـ`UPDLOCK, HOLDLOCK`. تشترط:

- المحاولة موجودة.
- حالتها `InProgress`.
- وقت SQL UTC الحالي أقل من `DeadlineAtUtc`؛ الوصول إلى الموعد النهائي أو تجاوزه يمنع حفظ الإجابة.
- السؤال يتبع المحاولة نفسها، لا مجرد وجود `ExamQuestionID`.
- الخيار له حقل خيار غير null في النسخة المجمدة، وTrueFalse يقبل `A/B` فقط.

يُحدّث `SelectedOption`, و`AnsweredAtUtc` أو يمسحه عند null، ويعيد `IsCorrect` إلى null حتى لا تُصحح الإجابة قبل التسليم. يجب أن يتأثر صف واحد بالضبط، وإلا تُرفض العملية. النجاح يرجع `ExamAttemptID`, `ExamQuestionID`, `Saved = true`، ولا يعيد صحة الإجابة.

خطأ صيغة الإجابة يرجع `400`. المحاولة المغلقة أو المنتهية أو السؤال غير المطابق يرجع `409` بحسب معالجة Controller الحالية. لا يعتمد حفظ الإجابة على وقت يرسله العميل.

## 13. التسليم والتصحيح وحفظ نتيجة DVLD

المسار:

```text
OfficialExamsController.Submit
  → clsOfficialExamSession.SubmitExam
  → clsOfficialExamSessionData.SubmitExam
```

لا يستقبل Submit درجة أو إجابات صحيحة أو مدة من العميل؛ يعمل على الإجابات التي سبق أن حفظها `SaveAnswer`.

تستخدم DataAccess معاملة Serializable وتنفذ:

1. قراءة وقفل المحاولة؛ المحاولة المفقودة أو غير القابلة للتسليم تُرفض.
2. إذا كانت `Submitted` أو `TimedOut` بالفعل، تعيد النتيجة المحفوظة وتُجري `Commit` دون إنشاء Test جديد. بذلك تكرار Submit بعد فقدان رد الشبكة لا ينشئ نتيجة ثانية.
3. تقفل الموعد وتتحقق أنه موجود وغير مقفل، ثم تتأكد أنه لا توجد نتيجة قديمة في `Tests` للموعد.
4. تصحح كل صف في `OfficialExamQuestions`: الإجابة صحيحة إذا كانت غير null وتساوي `CorrectOption`؛ غير المجاب يُعد خطأ.
5. تحسب `CorrectAnswers`، وتحدد `Passed = CorrectAnswers >= RequiredCorrectAnswers`.
6. تحدد الحالة `TimedOut` إذا كان وقت SQL الحالي عند قراءة المحاولة بلغ deadline أو تجاوزه، وإلا `Submitted`.
7. تنشئ سجلًا في `Tests` يحتوي الموعد والنجاح وNotes تلخص الدرجة والحالة.
8. تستخدم `TestAppointments.CreatedByUserID` كمنشئ النتيجة، لأن هذا حقل مطلوب في نموذج DVLD الحالي. هذا ليس تسجيل دخول جديدًا للممتحن.
9. تقفل الموعد `IsLocked = 1`، وتحدث المحاولة بالنتيجة و`TestID` ووقت الانتهاء.
10. تحفظ جميع العمليات بـ`Commit`، أو ترجعها كلها بـ`Rollback` عند أي خطأ.

الرد `FinalResult` يحتوي:

```text
ExamAttemptID, TestAppointmentID, TestID,
QuestionCount, RequiredCorrectAnswers, CorrectAnswers,
Passed, Status, FinishedAtUtc
```

الوقت المنتهي لا يجعل `Passed = false` تلقائيًا؛ يُصحح ما حُفظ من إجابات وتُطبق عتبة النجاح نفسها، وتكون الحالة `TimedOut`. يستطيع Submit إنهاء محاولة انتهى وقتها، لكن SaveAnswer لا يستطيع قبول إجابة جديدة بعد انتهاء الوقت.

لم يظهر Worker أو عملية دورية تُسلّم المحاولات المنتهية تلقائيًا في المشروع الحالي. ظهور `TimedOut` في الجدول أو الكود لا يثبت وجود إغلاق آلي عندما يغلق العميل دون استدعاء Submit. لم يظهر أيضًا endpoint لاستئناف امتحان قائم أو GET لجلب جلسة كاملة أو إلغاء المحاولة ضمن Controller الحالي.

## 14. الجداول والقيود الموجودة في ملف SQL

المصدر هو [OfficialExam_Muhannad_Migration.sql](<J:/gradeat project/DVLD Project Final/Project/Database/Migrations/OfficialExam_Muhannad_Migration.sql>). هذا الملف يصف إنشاء الجدولين في قاعدة التطوير `DVLD_Grad` ويتطلب جداول DVLD و`QuestionBank` الموجودة مسبقًا؛ لا يمثل إنشاء قاعدة المشروع كاملة.

- `OfficialExamAttempts`: المحاولة، الموعد، الشخص، الحالة، العدد، حد النجاح، المدة، UTC البداية والنهاية والانتهاء، الدرجة، النجاح، `TestID`, و`RowVersion`.
- `OfficialExamQuestions`: السؤال المجمد، ترتيبه ونوعه ونصه وخياراته، الإجابة الصحيحة الداخلية والتفسير ومصدر الوثيقة والصفحة، اختيار الممتحن ووقت الإجابة وصحتها عند التصحيح.
- يوجد Unique على `TestAppointmentID`: محاولة رسمية واحدة لكل موعد. الإعادة تحتاج موعدًا جديدًا ضمن workflow إعادة الاختبار.
- توجد مفاتيح أجنبية للمحاولة نحو الموعد والشخص والاختبار، وللسؤال نحو المحاولة و`QuestionBank`.
- توجد Unique لمنع تكرار رقم السؤال أو نفس سؤال البنك داخل محاولة واحدة.
- حالات المحاولة المسموحة في المخطط `InProgress / Submitted / TimedOut / Cancelled`. وجود `Cancelled` في القيد لا يعني وجود API إلغاء مكتملة.
- إذا كان `ReviewStatus` مفقودًا من `QuestionBank` يضيفه مع القيمة الافتراضية `Draft`؛ لا يعتمد أسئلة تلقائيًا.

هذه قيود مؤكدة في ملف SQL فقط. لم يُتحقق أنها طُبّقت على قاعدة البيانات الحالية أو على نسخة Backup المرفقة. الـsnapshot يحفظ نص الامتحان عند تعديل السؤال لاحقًا، لكن وجود FK إلى QuestionBank يعني أن حذف سجل البنك المرتبط يحتاج التعامل مع القيد؛ نسخة النص وحدها لا تعني سماح SQL بالحذف.

## 15. الموظف مقابل المواطن: ما هو موجود فعلًا

### 15.1 تسجيل دخول موظف Desktop

[DVLD/Program.cs](<J:/gradeat project/DVLD Project Final/Project/DVLD/Program.cs>) يبدأ التطبيق عبر `Application.Run(new frmLogin())`.

في [frmLogin.btnLogin_Click](<J:/gradeat project/DVLD Project Final/Project/DVLD/Login/frmLogin.cs>) تُقرأ بيانات الشاشة ثم:

```text
clsUser.FindByUsernameAndPassword
  → clsUserData.GetUserInfoByUsernameAndPassword
  → Users WHERE Username = @Username AND Password = @Password
```

المراجع: [clsUser.cs](<J:/gradeat project/DVLD Project Final/Project/DVLD_Buisness/clsUser.cs>) و[clsUserData.cs](<J:/gradeat project/DVLD Project Final/Project/DVLD_DataAccess/clsUserData.cs>).

إذا كان المستخدم غير موجود تظهر رسالة خطأ. وإذا وجد ولكنه `IsActive = false` يمنع الدخول. وإذا وجد ونشط تُوضع بياناته في `clsGlobal.CurrentUser`، وتُخفى شاشة الدخول ويُفتح `frmMain` باستخدام `ShowDialog()`.

`CurrentUser` موجود في [Global Classes/clsGlobal.cs](<J:/gradeat project/DVLD Project Final/Project/DVLD/Global Classes/clsGlobal.cs>). تستعمله الشاشات لوضع `CreatedByUserID` أو عرض اسم الموظف. هذا يثبت وجود دخول موظف وتسجيل صاحب العمليات؛ لا يثبت وجود أدوار أو صلاحيات تفصيلية لكل شاشة.

مسار الدخول الظاهر يقارن كلمة المرور مباشرة مع عمود `Password`، ولا يظهر Hash/Salt في هذه السلسلة. خيار Remember Me يستدعي `RememberUsernameAndPassword` و`GetStoredCredential`؛ لا تُعد البيانات المحفوظة أو UserID بمفردها Token صالحًا للـAPI.

### 15.2 تسجيل دخول المواطن وحماية API — تحديث 2026-10-09

أضيف MobileAuthController.Login ومصادقة Cookie وإعادة فحص الحساب بكل طلب. قوائم وتفاصيل الرخص المحلية والدولية والطلبات والمواعيد عليها Authorize وتتحقق من PersonID المصادق عليه قبل إعادة البيانات. طلب سجل شخص آخر يرجع404، والطلب دون جلسة يرجع401.

PeopleController مغلق بسياسة EmployeeApiOnly: الزائر401 والمواطن403؛ لا توجد مصادقة موظف API جاهزة في هذه السياسة. OfficialExamsController بقي خارج جلسة المواطن، ومخصص لمركز امتحان مشرف عليه؛ الرقم الوطني ليس تسجيل دخول لهذا المسار. راجع [الفصل 06](06-mobile-and-citizen-auth.md) للدوال والإعدادات وتدفق Android.

## 16. معالجة الأخطاء والحدود المؤكدة

ما يبدو سليمًا في المسارات المقروءة:

- الاستعلامات الأساسية للرخص والطلبات والمواعيد تستخدم معاملات مثل `@PersonID` و`@DriverID`، وليس تركيب قيم المدخلات داخل SQL.
- `404` عند غياب السجل الأساسي، وقائمة فارغة عند وجود الشخص دون سجلات، نتائج مفهومة مختلفة.
- العلاقات الاختيارية في قوائم الطلبات والمواعيد تستخدم `LEFT JOIN` وتحويل `DBNull`.
- معظم دوال القراءة المرتبطة بهذه الـAPIs تعيد رمي الاستثناءات، فتصل إلى معالج API المركزي.
- عمليات الامتحان مجمدة ومعاملاتها مترابطة، والتصحيح ووقت القبول يقررهما الخادم.

قيود ظاهرة من الكود:

| الموضع | السلوك الحالي والأثر |
|---|---|
| `clsLocalDrivingLicenseApplicationData.GetLocalDrivingLicenseApplicationInfoByApplicationID` | يمسك الاستثناء ويعيد `false`، فيمكن أن تظهر `Details = null` بدل خطأ قراءة |
| `clsLicenseClassData.GetLicenseClassInfoByID` داخل `LicenseClass.cs` | يمسك الاستثناء ويعيد `false`؛ قد تظهر معلومات الفئة فارغة |
| Controllers القراءة | لا يوجد رفض صريح للمعرفات العددية الصفرية أو السالبة قبل SQL في هذه الدوال؛ تُعامل غالبًا كسجلات غير موجودة |
| تفاصيل Retake | `TestID = -1` ممكن، بينما قائمة المواعيد تستعمل null عند غيابه |
| قائمة الرخص | فحص الحجز استعلام مستقل لكل رخصة؛ أثر الأداء الفعلي لم يُقَس |
| `clsInternationalLicense.Find` | يفترض وجود الطلب المرتبط وقد يرمي NullReference إذا كانت العلاقة مفقودة |
| صحة قاعدة البيانات | تمت قراءة أسماء الأعمدة والاستعلامات والمخطط المتاح، ولم يُفحص المخطط الحقيقي أو البيانات أو صلاحيات SQL |
| وقت الموعد | بعض الفحص في عملية API وبعضه النهائي في SQL محلي؛ صحة المناطق الزمنية وقت التشغيل غير مؤكدة |
| واجهة الكمبيوتر للامتحان | backend موجود، لكن لم تظهر ضمن المشروع الحالي شاشة تستدعي المسارات الرسمية؛ قد تكون واجهة منفصلة خارج النسخة المقروءة |

هذا الفصل لا يقترح إعادة كتابة APIs التي جُربت وظيفيًا. يشرح كيف تعمل النسخة الحالية، وما الذي يثبته الكود، وما الذي يحتاج تجربة تشغيل أو ملف واجهة مستقل أو فحص قاعدة بيانات حتى يصبح مؤكدًا.

## 17. صياغة مختصرة لشرح هذا الجزء في المناقشة

«عند طلب المواطن معلومات الرخص أو الطلبات أو المواعيد، يستقبل Controller المعرّف وينادي Business، ثم DataAccess ينفذ استعلامًا بمعاملات SQL. ترجع البيانات إلى Controller ليعيد حقولًا محددة في JSON. في الامتحان الرسمي، السياسة مصدرها إعدادات الخادم، وتُعاد مراجعة أهلية الموعد داخل معاملة SQL. تُختار أسئلة معتمدة وتُجمّد في جدول خاص، وتُرسل للممتحن بدون الإجابة الصحيحة. كل إجابة تُحفظ قبل انتهاء الوقت، ثم يصحح الخادم الإجابات المحفوظة، ويُنشئ نتيجة DVLD ويقفل الموعد ضمن معاملة واحدة. دور الـAI يسبق الامتحان في توليد الأسئلة المقترحة، ثم يراجعها الموظف قبل اعتمادها.»

مصادقة المواطن وملكية البيانات موثقتان في تحديث الفصل 06؛ الصلاحيات التفصيلية وواجهة الامتحان الخارجية ليستا مثبتتين كمكتملتين في هذا الدليل.
