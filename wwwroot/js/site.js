function UpdUsername(userid, username) {
    $.get(`/General/UpdateUsername?userid=${userid}&username=${username}`).then(() => {
        const Toast = Swal.mixin({
            toast: true,
            position: 'bottom-end',
            showConfirmButton: false,
            timer: 1500,
            timerProgressBar: true,
            didOpen: (toast) => {
                toast.addEventListener('mouseenter', Swal.stopTimer)
                toast.addEventListener('mouseleave', Swal.resumeTimer)
            }
        })
        Toast.fire({
            icon: 'success',
            title: 'Имя пользователя успешно обновлено!'
        }).then(() => {
            window.location.href = '/General/Account/';
        });
    }).catch(() => {
        Swal.fire({
            icon: 'error',
            title: 'Ошибка!',
            text: 'Не получилось обновить имя пользователя! Повторите попытку позже.',
            buttonsStyling: true,
            confirmButtonColor: "#8DD7AB"
        }).then(() => {
            window.location.href = '/General/Account/';
        });
    });
};

function CreProject(userid, name, desc, start, end) {
    $.get(`/Partial/CreateNewProject?userid=${userid}&name=${name}&desc=${desc}&start=${start}&end=${end}`).then(() => {
        const Toast = Swal.mixin({
            toast: true,
            position: 'bottom-end',
            showConfirmButton: false,
            timer: 1500,
            timerProgressBar: true,
            didOpen: (toast) => {
                toast.addEventListener('mouseenter', Swal.stopTimer)
                toast.addEventListener('mouseleave', Swal.resumeTimer)
            }
        })
        Toast.fire({
            icon: 'success',
            title: 'Проект успешно был создан'
        }).then(() => {
            window.location.href = '/Action/Projects';
        });
    }).catch(() => {
        const Toast = Swal.mixin({
            toast: true,
            position: 'bottom-end',
            showConfirmButton: false,
            timer: 1500,
            timerProgressBar: true,
            didOpen: (toast) => {
                toast.addEventListener('mouseenter', Swal.stopTimer)
                toast.addEventListener('mouseleave', Swal.resumeTimer)
            }
        })
        Toast.fire({
            icon: 'error',
            title: 'Не получилось создать новый проект! Повторите попытку позже'
        }).then(() => {
            window.location.href = '/Action/Projects';
        });
    });
};

function pdfsave(userid) {
    window.jsPDF = window.jspdf.jsPDF;
    google.charts.load('current', { 'packages': ['corechart'] }).then(function () {
        $.ajax({
            url: `/Home/GetCompletedTasksData?userid=${userid}`,
            method: 'GET',
            success: function (data) {
                var chartData = new google.visualization.DataTable();
                chartData.addColumn('string', 'Месяц');
                chartData.addColumn('number', 'Выполненные задачи');

                data.forEach(function (item) {
                    chartData.addRow([item.month, item.completedTasks]);
                });

                var options = {
                    title: 'Статистика выполненных задач по месяцам',
                    curveType: 'none',
                    vAxis: { minValue: 0 },
                    chartArea: { width: '2000px', height: '600px' },
                    titleTextStyle: {
                        fontSize: 16,
                        bold: false,
                        color: 'black'
                    },
                    series: {
                        0: {
                            lineWidth: 2
                        }
                    },
                    isStacked: 'absolute',
                    legend: { position: 'bottom' }
                };

                var chart = new google.visualization.LineChart(document.getElementById('chartfortasks2'));
                chart.draw(chartData, options);

                setTimeout(function () {
                    var doc = new jsPDF();
                    doc.addImage(chart.getImageURI(), 10, 10);
                    doc.output('dataurlnewwindow');
                }, 500);
            }
        });
    });
};

function pdfsave2(userid) {
    window.jsPDF = window.jspdf.jsPDF;
    google.charts.load('current', { 'packages': ['corechart'] }).then(function () {
        $.ajax({
            url: `/Home/GetCompletedProjectsData?userid=${userid}`,
            method: 'GET',
            success: function (data) {
                var chartData = new google.visualization.DataTable();
                chartData.addColumn('string', 'Месяц');
                chartData.addColumn('number', 'Выполненные проекты');

                data.forEach(function (item) {
                    chartData.addRow([item.month, item.completedTasks]);
                });

                var options = {
                    title: 'Статистика выполненных проектов по месяцам',
                    curveType: 'none',
                    vAxis: { minValue: 0 },
                    chartArea: { width: '2000px', height: '600px' },
                    titleTextStyle: {
                        fontSize: 16,
                        bold: false,
                        color: 'black'
                    },
                    series: {
                        0: {
                            lineWidth: 2
                        }
                    },
                    isStacked: 'absolute',
                    legend: { position: 'bottom' }
                };

                var chart = new google.visualization.LineChart(document.getElementById('chartforprojects2'));
                chart.draw(chartData, options);

                setTimeout(function () {
                    var doc = new jsPDF();
                    doc.addImage(chart.getImageURI(), 10, 10);
                    doc.output('dataurlnewwindow');
                }, 500);
            }
        });
    });
};

function CreTask(userId, projectId, taskname, taskdesc, taskpriority, taskstartdate, taskenddate) {
    var formData = new FormData();
    formData.append("taskname", taskname);
    formData.append("taskdesc", taskdesc);
    formData.append("taskpriority", taskpriority);
    formData.append("taskstartdate", taskstartdate);
    formData.append("taskenddate", taskenddate);
    formData.append("userId", userId);
    formData.append("projectId", projectId);

    var filesInput = document.getElementById("files");
    for (var i = 0; i < filesInput.files.length; i++) {
        formData.append("files", filesInput.files[i]);
    }

    // Отправляем данные на сервер
    $.ajax({
        url: '/Partial/CreateNewTask', // Путь к вашему методу контроллера
        type: 'POST',
        data: formData,
        processData: false, // Отключаем автоматическую обработку данных
        contentType: false, // Отключаем автоматическое установление Content-Type
        success: function (response) {
            const Toast = Swal.mixin({
                toast: true,
                position: 'bottom-end',
                showConfirmButton: false,
                timer: 1500,
                timerProgressBar: true,
                didOpen: (toast) => {
                    toast.addEventListener('mouseenter', Swal.stopTimer)
                    toast.addEventListener('mouseleave', Swal.resumeTimer)
                }
            })
            Toast.fire({
                icon: 'success',
                title: 'Задача успешно создана'
            }).then(() => {
                window.location.href = `/Action/Project?projid=${projectId}`;
            });
        },
        error: function (error) {
            const Toast = Swal.mixin({
                toast: true,
                position: 'bottom-end',
                showConfirmButton: false,
                timer: 1500,
                timerProgressBar: true,
                didOpen: (toast) => {
                    toast.addEventListener('mouseenter', Swal.stopTimer)
                    toast.addEventListener('mouseleave', Swal.resumeTimer)
                }
            })
            Toast.fire({
                icon: 'error',
                title: 'Не получилось создать новую задачу! Повторите попытку позже'
            }).then(() => {
                window.location.href = `/Action/Project?projid=${projectId}`;
            });
        }
    });
};

function EdProject(projectId, projname, projdesc, projstartdate, projenddate, projstatus) {
    $.get(`/Partial/EditingProject?projectId=${projectId}&projname=${projname}&projdesc=${projdesc}&projstartdate=${projstartdate}&projenddate=${projenddate}&projstatus=${projstatus}`).then(() => {
        const Toast = Swal.mixin({
            toast: true,
            position: 'bottom-end',
            showConfirmButton: false,
            timer: 1500,
            timerProgressBar: true,
            didOpen: (toast) => {
                toast.addEventListener('mouseenter', Swal.stopTimer)
                toast.addEventListener('mouseleave', Swal.resumeTimer)
            }
        })
        Toast.fire({
            icon: 'success',
            title: 'Редактирование проекта прошло успешно!'
        }).then(() => {
            window.location.href = `/Action/Project?projid=${projectId}`;
        });
    }).catch(() => {
        const Toast = Swal.mixin({
            toast: true,
            position: 'bottom-end',
            showConfirmButton: false,
            timer: 1500,
            timerProgressBar: true,
            didOpen: (toast) => {
                toast.addEventListener('mouseenter', Swal.stopTimer)
                toast.addEventListener('mouseleave', Swal.resumeTimer)
            }
        })
        Toast.fire({
            icon: 'error',
            title: 'Не получилось отредактировать проект! Повторите попытку позже'
        }).then(() => {
            window.location.href = `/Action/Project?projid=${projectId}`;
        });
    });
};

function EdTask(taskid, taskname, taskstatus, taskpriority, taskdesc, taskstartdate, taskenddate) {
    $.get(`/Partial/EditingTask?taskid=${taskid}&taskname=${taskname}&taskstatus=${taskstatus}&taskpriority=${taskpriority}&taskdesc=${taskdesc}&taskstartdate=${taskstartdate}&taskenddate=${taskenddate}`).then(() => {
        const Toast = Swal.mixin({
            toast: true,
            position: 'bottom-end',
            showConfirmButton: false,
            timer: 1500,
            timerProgressBar: true,
            didOpen: (toast) => {
                toast.addEventListener('mouseenter', Swal.stopTimer)
                toast.addEventListener('mouseleave', Swal.resumeTimer)
            }
        })
        Toast.fire({
            icon: 'success',
            title: 'Редактирование задачи прошло успешно!'
        }).then(() => {
            location.reload();
        });
    }).catch(() => {
        const Toast = Swal.mixin({
            toast: true,
            position: 'bottom-end',
            showConfirmButton: false,
            timer: 1500,
            timerProgressBar: true,
            didOpen: (toast) => {
                toast.addEventListener('mouseenter', Swal.stopTimer)
                toast.addEventListener('mouseleave', Swal.resumeTimer)
            }
        })
        Toast.fire({
            icon: 'error',
            title: 'Не получилось отредактировать задачу! Повторите попытку позже'
        }).then(() => {
            location.reload();
        });
    });
};

function ProjectSorting(name, date, status) {
    $.get(`/Action/Projects?name=${name}&date=${date}&status=${status}`).then(_ => {
        window.location.href = `/Action/Projects?name=${name}&date=${date}&status=${status}`;
    });
};

function TasksSorting(namesort, creationdatesort, statussort, prioritysort) {
    $.get(`/Action/Tasks?namesort=${namesort}&creationdatesort=${creationdatesort}&statussort=${statussort}&prioritysort=${prioritysort}`).then(_ => {
        window.location.href = `/Action/Tasks?namesort=${namesort}&creationdatesort=${creationdatesort}&statussort=${statussort}&prioritysort=${prioritysort}`;
    });
};

function DelFile(fileid) {
    $.get(`/Partial/DeleteFileInTask?file=${fileid}`).then(() => {
        const Toast = Swal.mixin({
            toast: true,
            position: 'bottom-end',
            showConfirmButton: false,
            timer: 1500,
            timerProgressBar: true,
            didOpen: (toast) => {
                toast.addEventListener('mouseenter', Swal.stopTimer)
                toast.addEventListener('mouseleave', Swal.resumeTimer)
            }
        })
        Toast.fire({
            icon: 'success',
            title: 'Файл успешно удален!'
        }).then(() => {
            location.reload();
        });
    }).catch(() => {
        const Toast = Swal.mixin({
            toast: true,
            position: 'bottom-end',
            showConfirmButton: false,
            timer: 1500,
            timerProgressBar: true,
            didOpen: (toast) => {
                toast.addEventListener('mouseenter', Swal.stopTimer)
                toast.addEventListener('mouseleave', Swal.resumeTimer)
            }
        })
        Toast.fire({
            icon: 'error',
            title: 'Не получилось удалить файл! Повторите попытку позже.'
        });
    });
};

function DelTask(taskid) {
    $.get(`/Partial/DeleteTask?taskid=${taskid}`).then(() => {
        const Toast = Swal.mixin({
            toast: true,
            position: 'bottom-end',
            showConfirmButton: false,
            timer: 1500,
            timerProgressBar: true,
            didOpen: (toast) => {
                toast.addEventListener('mouseenter', Swal.stopTimer)
                toast.addEventListener('mouseleave', Swal.resumeTimer)
            }
        })
        Toast.fire({
            icon: 'success',
            title: 'Задача успешно удалена!'
        }).then(() => {
            location.reload();
        });
    }).catch(() => {
        const Toast = Swal.mixin({
            toast: true,
            position: 'bottom-end',
            showConfirmButton: false,
            timer: 1500,
            timerProgressBar: true,
            didOpen: (toast) => {
                toast.addEventListener('mouseenter', Swal.stopTimer)
                toast.addEventListener('mouseleave', Swal.resumeTimer)
            }
        })
        Toast.fire({
            icon: 'error',
            title: 'Не получилось удалить задачу! Повторите попытку позже.'
        });
    });
};

function ChgTaskStatus(id, status) {
    $.get(`/Action/ChangeTaskStatus?taskid=${id}&status=${status}`).then(() => {
        location.reload();
    }).catch(() => {
        location.reload();
    });
};

function UpdNotiRules() {
    var checkboxes = document.querySelectorAll('input[type="checkbox"]');
    var selectedNotifications = [];

    checkboxes.forEach(function (checkbox) {
        var notiId = checkbox.getAttribute('name');
        var isWork = checkbox.checked;

        selectedNotifications.push({ id: notiId, isWork: isWork });
    });

    var jsonData = JSON.stringify(selectedNotifications);
    console.log(jsonData);

    $.ajax({
        url: '/General/UpdateNotifications',
        type: 'POST',
        contentType: 'application/json',
        data: jsonData,
        success: function (response) {
            const Toast = Swal.mixin({
                toast: true,
                position: 'bottom-end',
                showConfirmButton: false,
                timer: 3000,
                timerProgressBar: true,
                didOpen: (toast) => {
                    toast.addEventListener('mouseenter', Swal.stopTimer)
                    toast.addEventListener('mouseleave', Swal.resumeTimer)
                }
            })
            Toast.fire({
                icon: 'success',
                title: 'Настройки уведомлений успешно обновлены!'
            });
        },
        error: function (error) {
            const Toast = Swal.mixin({
                toast: true,
                position: 'bottom-end',
                showConfirmButton: false,
                timer: 3000,
                timerProgressBar: true,
                didOpen: (toast) => {
                    toast.addEventListener('mouseenter', Swal.stopTimer)
                    toast.addEventListener('mouseleave', Swal.resumeTimer)
                }
            })
            Toast.fire({
                icon: 'error',
                title: 'Не получилось обновить настройки. Повторите попытку позже('
            });
        }
    });
    
};

function UpdPassword(userid, oldpass, newpass, newpassrep) {
    if (newpass === newpassrep) {
        $.get(`/General/UpdatePassword?userid=${userid}&newpass=${newpass}&oldpass=${oldpass}`, function (responseText) {
            if (responseText === 2) {
                const Toast = Swal.mixin({
                    toast: true,
                    position: 'bottom-end',
                    showConfirmButton: false,
                    timer: 1500,
                    timerProgressBar: true,
                    didOpen: (toast) => {
                        toast.addEventListener('mouseenter', Swal.stopTimer)
                        toast.addEventListener('mouseleave', Swal.resumeTimer)
                    }
                })
                Toast.fire({
                    icon: 'error',
                    title: 'Введен неверный старый пароль, проверьте, пожалуйста, вводимые значения.'
                });
            }
            if (responseText === 3) {
                const Toast = Swal.mixin({
                    toast: true,
                    position: 'bottom-end',
                    showConfirmButton: false,
                    timer: 1500,
                    timerProgressBar: true,
                    didOpen: (toast) => {
                        toast.addEventListener('mouseenter', Swal.stopTimer)
                        toast.addEventListener('mouseleave', Swal.resumeTimer)
                    }
                })
                Toast.fire({
                    icon: 'error',
                    title: 'Новый пароль равен старому, просим использовать новый пароль отличный от старого.'
                });
            }
            if (responseText === 1) {
                const Toast = Swal.mixin({
                    toast: true,
                    position: 'bottom-end',
                    showConfirmButton: false,
                    timer: 1500,
                    timerProgressBar: true,
                    didOpen: (toast) => {
                        toast.addEventListener('mouseenter', Swal.stopTimer)
                        toast.addEventListener('mouseleave', Swal.resumeTimer)
                    }
                })
                Toast.fire({
                    icon: 'success',
                    title: 'Пароль успешно изменен!'
                }).then(() => {
                    window.location.href = '/General/Account';
                });
            }
        }).catch(() => {
            const Toast = Swal.mixin({
                toast: true,
                position: 'bottom-end',
                showConfirmButton: false,
                timer: 1500,
                timerProgressBar: true,
                didOpen: (toast) => {
                    toast.addEventListener('mouseenter', Swal.stopTimer)
                    toast.addEventListener('mouseleave', Swal.resumeTimer)
                }
            })
            Toast.fire({
                icon: 'error',
                title: 'Не получилось изменить пароль! Повторите попытку позже.'
            });
        });
    }
    else {
        const Toast = Swal.mixin({
            toast: true,
            position: 'bottom-end',
            showConfirmButton: false,
            timer: 1500,
            timerProgressBar: true,
            didOpen: (toast) => {
                toast.addEventListener('mouseenter', Swal.stopTimer)
                toast.addEventListener('mouseleave', Swal.resumeTimer)
            }
        })
        Toast.fire({
            icon: 'error',
            title: 'Ваши пароли отличаются, проверьте ввод в полях "новый пароль" и "повторение нового пароля".'
        });
    }
};

function RmvAccount(userid, useremail) {
    Swal.fire({
        title: 'Введите адрес электронной почты для подтверждения',
        input: 'text',
        inputAttributes: {
            autocapitalize: 'off'
        },
        showDenyButton: true,
        confirmButtonText: 'Подтвердить',
        confirmButtonColor: "#E56F8C",
        denyButtonText: `Отмена`,
        denyButtonColor: "#240A0B",
        showLoaderOnConfirm: true,
        preConfirm: (login) => {
            if (login === useremail) {
                return true;
            }
            else {
                return false;
            }
        },
        allowOutsideClick: () => !Swal.isLoading()
    }).then((result) => {
        console.log(result);
        if (result.isConfirmed) {
            $.get(`/General/RemoveAccount?userid=${userid}`).then(() => {
                const Toast = Swal.mixin({
                    toast: true,
                    position: 'bottom-end',
                    showConfirmButton: false,
                    timer: 1500,
                    timerProgressBar: true,
                    didOpen: (toast) => {
                        toast.addEventListener('mouseenter', Swal.stopTimer)
                        toast.addEventListener('mouseleave', Swal.resumeTimer)
                    }
                })
                Toast.fire({
                    icon: 'error',
                    title: 'Аккаунт успешно удален, возвращайтесь к нам снова!'
                }).then(() => {
                    window.location.href = '/Logon/Login';
                });
            });
        }
    })
};

function Logout() {
    $.get(`/General/UserLogout`).then(() => {
        const Toast = Swal.mixin({
            toast: true,
            position: 'bottom-end',
            showConfirmButton: false,
            timer: 1000,
            timerProgressBar: true
        })
        Toast.fire({
            icon: 'success',
            title: 'Возвращайтесь снова!'
        }).then(() => {
            window.location.href = '/Logon/Login';
        });
    }).catch(() => {
        const Toast = Swal.mixin({
            toast: true,
            position: 'bottom-end',
            showConfirmButton: false,
            timer: 1500,
            timerProgressBar: true
        })
        Toast.fire({
            icon: 'error',
            title: 'Произошла ошибка! Повторите попытку позже('
        });
    });
};

function openModalCreateProject(parameters) {
    const id = parameters.data;
    const url = parameters.url;
    const modal = $('#modal');

    $.ajax({
        type: 'GET',
        url: url,
        data: { "id": id },
        success: function (response) {
            $('.modal-dialog');
            modal.find(".modal-body").html(response);
            modal.modal('show');
        },
        failure: function () {
            modal.modal('hide');
        }
    }).then(() => {
        let containerElement = document.querySelector('#modal');
        containerElement.setAttribute('style', 'display: flex !important');
        console.log(document.querySelector('#modal').style.display);
    });
};

function openModalEditProject(options) {
    const projectid = options.projectid;
    const url = options.url;
    const modal = $('#modal');

    $.ajax({
        type: 'GET',
        url: url,
        data: { "projectid": projectid },
        success: function (response) {
            $('.modal-dialog');
            modal.find(".modal-body").html(response);
            modal.modal('show');
        },
        failure: function () {
            modal.modal('hide');
        }
    }).then(() => {
        let containerElement = document.querySelector('#modal');
        containerElement.setAttribute('style', 'display: flex !important');
        console.log(document.querySelector('#modal').style.display);
    });
};

function openModalCreateTask(options) {
    var url = options.url;
    var projectId = options.projectId;
    const modal = $('#modal');

    $.ajax({
        type: 'GET',
        url: url,
        data: { "projectid": projectId },
        success: function (response) {
            $('.modal-dialog');
            modal.find(".modal-body").html(response);
            modal.modal('show');
        },
        failure: function () {
            modal.modal('hide');
        }
    }).then(() => {
        let containerElement = document.querySelector('#modal');
        containerElement.setAttribute('style', 'display: flex !important');
        console.log(document.querySelector('#modal').style.display);
    });
};

function openModalReadTask(options) {
    var url = options.url;
    var taskId = options.data;
    const modal = $('#modal');

    $.ajax({
        type: 'GET',
        url: url,
        data: { "taskId": taskId },
        success: function (response) {
            $('.modal-dialog');
            modal.find(".modal-body").html(response);
            modal.modal('show');
        },
        failure: function () {
            modal.modal('hide');
        }
    }).then(() => {
        let containerElement = document.querySelector('#modal');
        containerElement.setAttribute('style', 'display: flex !important');
        console.log(document.querySelector('#modal').style.display);
    });
};

function openModalEditTask(options) {
    var url = options.url;
    var taskId = options.data;
    const modal = $('#modal');

    $.ajax({
        type: 'GET',
        url: url,
        data: { "taskId": taskId },
        success: function (response) {
            $('.modal-dialog');
            modal.find(".modal-body").html(response);
            modal.modal('show');
        },
        failure: function () {
            modal.modal('hide');
        }
    }).then(() => {
        let containerElement = document.querySelector('#modal');
        containerElement.setAttribute('style', 'display: flex !important');
        console.log(document.querySelector('#modal').style.display);
    });
};

function openModalOpenLogs(parameters) {
    const id = parameters.data;
    const url = parameters.url;
    const modal = $('#modal');

    $.ajax({
        type: 'GET',
        url: url,
        data: { "id": id },
        success: function (response) {
            $('.modal-dialog');
            modal.find(".modal-body").html(response);
            modal.modal('show');
        },
        failure: function () {
            modal.modal('hide');
        }
    }).then(() => {
        let containerElement = document.querySelector('#modal');
        containerElement.setAttribute('style', 'display: flex !important');
        console.log(document.querySelector('#modal').style.display);
    });
};

function openModalQrCode(parameters) {
    const link = window.location.protocol + '//' + window.location.host + '/' +  parameters.data;
    const url = parameters.url;
    const modal = $('#modal');

    $.ajax({
        type: 'GET',
        url: url,
        data: { "link": link },
        success: function (response) {
            $('.modal-dialog');
            modal.find(".modal-body").html(response);
            modal.modal('show');
        },
        failure: function () {
            modal.modal('hide');
        }
    }).then(() => {
        let containerElement = document.querySelector('#modal');
        containerElement.setAttribute('style', 'display: flex !important');
        console.log(document.querySelector('#modal').style.display);
    });
};

function RedirectTo(to) {
    var link = window.location.protocol + '//' + window.location.host + '/' + to;
    window.location.replace(link);
};

function OpenUploadFile(filename) {
    var url = "https://localhost:44301/uploads/" + filename;
    window.open(url, '_blank');
};

function SearchUser(name, email, role, status) {
    $.get(`/Admin/Index?name=${name}&email=${email}&role=${role}&status=${status}`).then(_ => {
        window.location.href = `/Admin/Index?name=${name}&email=${email}&role=${role}&status=${status}`;
    });
};

function SendNews(areaadminnews) {
    $.get(`/Admin/SendProLogsNews?message=${areaadminnews}`).then(() => {
        const Toast = Swal.mixin({
            toast: true,
            position: 'bottom-end',
            showConfirmButton: false,
            timer: 1500,
            timerProgressBar: true
        })
        Toast.fire({
            icon: 'success',
            title: 'Новость была успешно отправлена!'
        }).then(() => {
            window.location.href = '/Admin/Index';
        });
    }).catch(() => {
        const Toast = Swal.mixin({
            toast: true,
            position: 'bottom-end',
            showConfirmButton: false,
            timer: 1000,
            timerProgressBar: true
        })
        Toast.fire({
            icon: 'error',
            title: 'Произошла ошибка! Повторите попытку позже.'
        }).then(() => {
            window.location.href = '/Admin/Index';
        });
    });
}

function RecoverySendCode(email) {
    if (email !== "") {
        $.get(`/Logon/SendEmailForRecovery?email=${email}`).then(() => {
            var target = document.getElementById('lastelemonpage');
            
            var str = '<div class="mrtop-16"><input type="text" class="input-text-login" name="code" placeholder="Код подтверждения"></div><div class="mrtop-16"><input type="password" name="newpassword" class="input-text-login" placeholder="Новый пароль"></div><input type = "button" value = "Сменить пароль" onclick = "UpdatePassword(this.form.useremail.value, this.form.code.value, this.form.newpassword.value);" class="mrtop-16 input-button-login"></div>';
            var temp = document.createElement('div');
            temp.innerHTML = str;
            
            while (temp.firstChild) {
                target.appendChild(temp.firstChild);
            }

            var input = document.getElementById("useremail");
            var button = document.getElementById("emailbutton");

            // Disable the email input and the button
            input.disabled = true;
            button.disabled = true;

            const Toast = Swal.mixin({
                toast: true,
                position: 'bottom-end',
                showConfirmButton: false,
                timer: 1000,
                timerProgressBar: true
            })
            Toast.fire({
                icon: 'success',
                title: `${email} вам отправлен код для восстановления пароля! Проверьте свой почтовый ящик.`
            }).then(() => {
                document.getElementById("useremail").setAttribute("value", email);
            });
        }).catch(() => {
            const Toast = Swal.mixin({
                toast: true,
                position: 'bottom-end',
                showConfirmButton: false,
                timer: 1000,
                timerProgressBar: true
            })
            Toast.fire({
                icon: 'error',
                title: 'Не получилось отправь код восстановления! Повторите попытку позже.'
            }).then(() => {
                window.location.href = '/Logon/Login/';
            });
        });
    }
    else {
        const Toast = Swal.mixin({
            toast: true,
            position: 'bottom-end',
            showConfirmButton: false,
            timer: 1000,
            timerProgressBar: true
        })
        Toast.fire({
            icon: 'error',
            title: 'Введите адрес электронной почты!'
        });
    }
};

function UpdatePassword(email, code, newpassword) {
    $.get(`/Logon/UpdatePasswordInRecovery?email=${email}&code=${code}&newpassword=${newpassword}`).then(() => {
        const Toast = Swal.mixin({
            toast: true,
            position: 'bottom-end',
            showConfirmButton: false,
            timer: 1000,
            timerProgressBar: true
        })
        Toast.fire({
            icon: 'success',
            title: 'Ваш пароль был изменен!'
        }).then(() => {
            window.location.href = '/Logon/Login';
        });
    }).catch(() => {
        const Toast = Swal.mixin({
            toast: true,
            position: 'bottom-end',
            showConfirmButton: false,
            timer: 1000,
            timerProgressBar: true
        })
        Toast.fire({
            icon: 'error',
            title: 'Произошла ошибка! Повторите попытку позже.'
        }).then(() => {
            window.location.href = '/Logon/Login';
        });
    });
};

function createProjDateValidation() {
    projstartdate = document.getElementById('projstartdate');
    document.getElementById("projenddate").setAttribute("min", projstartdate.value);
};

function createTaskDateValidation() {
    taskstartdate = document.getElementById('taskstartdate');
    document.getElementById("taskenddate").setAttribute("min", taskstartdate.value);
};

function editProjDateValidation() {
    projstartdate = document.getElementById('projstartdate');
    document.getElementById("projenddate").setAttribute("min", projstartdate.value);
};