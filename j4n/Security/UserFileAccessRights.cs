using System;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;

namespace j4n.Security
{
    /// <span class="code-SummaryComment">
    ///     <summary>
    /// </span>
    /// Configuring a Web site through a Web interface can be tricky. 
    /// If one is to read and write various files, it is useful to know 
    /// in advance if you have the authority to do so.
    /// 
    /// This class contains a simple answer to a 
    /// potentially complicated question 
    /// "Can I read this file or can I write to this file?"
    /// 
    /// Using the "rule of least privilege", 
    /// one must check not only is access granted but 
    /// is it denied at any point including a possibly recursive check of groups.
    /// 
    /// For this simple check, a look at the user and immediate groups are only checked.
    /// 
    /// This class could be expanded to identify if the applicable allow/deny rule
    /// was explicit or inherited
    /// <span class="code-SummaryComment"></summary></span>
    public class UserFileAccessRights
    {
        private readonly bool _allowAppendData;
        private readonly bool _allowChangePermissions;
        private readonly bool _allowCreateDirectories;
        private readonly bool _allowCreateFiles;
        private readonly bool _allowDelete;
        private readonly bool _allowDeleteSubdirectoriesAndFiles;
        private readonly bool _allowExecuteFile;
        private readonly bool _allowFullControl;
        private readonly bool _allowListDirectory;
        private readonly bool _allowModify;
        private readonly bool _allowRead;
        private readonly bool _allowReadAndExecute;
        private readonly bool _allowReadAttributes;
        private readonly bool _allowReadData;
        private readonly bool _allowReadExtendedAttributes;
        private readonly bool _allowReadPermissions;
        private readonly bool _allowSynchronize;
        private readonly bool _allowTakeOwnership;
        private readonly bool _allowTraverse;
        private readonly bool _allowWrite;
        private readonly bool _allowWriteAttributes;
        private readonly bool _allowWriteData;
        private readonly bool _allowWriteExtendedAttributes;
        private readonly bool _denyAppendData;
        private readonly bool _denyChangePermissions;
        private readonly bool _denyCreateDirectories;
        private readonly bool _denyCreateFiles;
        private readonly bool _denyDelete;
        private readonly bool _denyDeleteSubdirectoriesAndFiles;
        private readonly bool _denyExecuteFile;
        private readonly bool _denyFullControl;
        private readonly bool _denyListDirectory;
        private readonly bool _denyModify;
        private readonly bool _denyRead;
        private readonly bool _denyReadAndExecute;
        private readonly bool _denyReadAttributes;
        private readonly bool _denyReadData;
        private readonly bool _denyReadExtendedAttributes;
        private readonly bool _denyReadPermissions;
        private readonly bool _denySynchronize;
        private readonly bool _denyTakeOwnership;
        private readonly bool _denyTraverse;
        private readonly bool _denyWrite;
        private readonly bool _denyWriteAttributes;
        private readonly bool _denyWriteData;
        private readonly bool _denyWriteExtendedAttributes;
        private readonly string _path;
        private readonly WindowsIdentity _principal;

        /// <span class="code-SummaryComment">
        ///     <summary>
        /// </span>
        /// Convenience constructor assumes the current user
        /// <span class="code-SummaryComment"></summary></span>
        /// <span class="code-SummaryComment">
        ///     <param name="path"></param>
        /// </span>
        public UserFileAccessRights(string path) :
            this(path, WindowsIdentity.GetCurrent())
        {
        }


        /// <span class="code-SummaryComment">
        ///     <summary>
        /// </span>
        /// Supply the path to the file or directory and a user or group. 
        /// Access checks are done
        /// during instantiation to ensure we always have a valid object
        /// <span class="code-SummaryComment"></summary></span>
        /// <span class="code-SummaryComment">
        ///     <param name="path"></param>
        /// </span>
        /// <span class="code-SummaryComment">
        ///     <param name="principal"></param>
        /// </span>
        public UserFileAccessRights(string path,
            WindowsIdentity principal)
        {
            _path = path;
            _principal = principal;

            try
            {
                var fi = new FileInfo(_path);
                AuthorizationRuleCollection acl = fi.GetAccessControl().GetAccessRules
                    (true, true, typeof (SecurityIdentifier));
                for (int i = 0; i < acl.Count; i++)
                {
                    var rule =
                        (FileSystemAccessRule) acl[i];
                    if (_principal.User.Equals(rule.IdentityReference))
                    {
                        if (AccessControlType.Deny.Equals
                            (rule.AccessControlType))
                        {
                            if (contains(FileSystemRights.AppendData, rule))
                                _denyAppendData = true;
                            if (contains(FileSystemRights.ChangePermissions, rule))
                                _denyChangePermissions = true;
                            if (contains(FileSystemRights.CreateDirectories, rule))
                                _denyCreateDirectories = true;
                            if (contains(FileSystemRights.CreateFiles, rule))
                                _denyCreateFiles = true;
                            if (contains(FileSystemRights.Delete, rule))
                                _denyDelete = true;
                            if (contains(FileSystemRights.DeleteSubdirectoriesAndFiles,
                                rule)) _denyDeleteSubdirectoriesAndFiles = true;
                            if (contains(FileSystemRights.ExecuteFile, rule))
                                _denyExecuteFile = true;
                            if (contains(FileSystemRights.FullControl, rule))
                                _denyFullControl = true;
                            if (contains(FileSystemRights.ListDirectory, rule))
                                _denyListDirectory = true;
                            if (contains(FileSystemRights.Modify, rule))
                                _denyModify = true;
                            if (contains(FileSystemRights.Read, rule)) _denyRead = true;
                            if (contains(FileSystemRights.ReadAndExecute, rule))
                                _denyReadAndExecute = true;
                            if (contains(FileSystemRights.ReadAttributes, rule))
                                _denyReadAttributes = true;
                            if (contains(FileSystemRights.ReadData, rule))
                                _denyReadData = true;
                            if (contains(FileSystemRights.ReadExtendedAttributes, rule))
                                _denyReadExtendedAttributes = true;
                            if (contains(FileSystemRights.ReadPermissions, rule))
                                _denyReadPermissions = true;
                            if (contains(FileSystemRights.Synchronize, rule))
                                _denySynchronize = true;
                            if (contains(FileSystemRights.TakeOwnership, rule))
                                _denyTakeOwnership = true;
                            if (contains(FileSystemRights.Traverse, rule))
                                _denyTraverse = true;
                            if (contains(FileSystemRights.Write, rule)) _denyWrite = true;
                            if (contains(FileSystemRights.WriteAttributes, rule))
                                _denyWriteAttributes = true;
                            if (contains(FileSystemRights.WriteData, rule))
                                _denyWriteData = true;
                            if (contains(FileSystemRights.WriteExtendedAttributes, rule))
                                _denyWriteExtendedAttributes = true;
                        }
                        else if (AccessControlType.
                            Allow.Equals(rule.AccessControlType))
                        {
                            if (contains(FileSystemRights.AppendData, rule))
                                _allowAppendData = true;
                            if (contains(FileSystemRights.ChangePermissions, rule))
                                _allowChangePermissions = true;
                            if (contains(FileSystemRights.CreateDirectories, rule))
                                _allowCreateDirectories = true;
                            if (contains(FileSystemRights.CreateFiles, rule))
                                _allowCreateFiles = true;
                            if (contains(FileSystemRights.Delete, rule))
                                _allowDelete = true;
                            if (contains(FileSystemRights.DeleteSubdirectoriesAndFiles,
                                rule)) _allowDeleteSubdirectoriesAndFiles = true;
                            if (contains(FileSystemRights.ExecuteFile, rule))
                                _allowExecuteFile = true;
                            if (contains(FileSystemRights.FullControl, rule))
                                _allowFullControl = true;
                            if (contains(FileSystemRights.ListDirectory, rule))
                                _allowListDirectory = true;
                            if (contains(FileSystemRights.Modify, rule))
                                _allowModify = true;
                            if (contains(FileSystemRights.Read, rule)) _allowRead = true;
                            if (contains(FileSystemRights.ReadAndExecute, rule))
                                _allowReadAndExecute = true;
                            if (contains(FileSystemRights.ReadAttributes, rule))
                                _allowReadAttributes = true;
                            if (contains(FileSystemRights.ReadData, rule))
                                _allowReadData = true;
                            if (contains(FileSystemRights.ReadExtendedAttributes, rule))
                                _allowReadExtendedAttributes = true;
                            if (contains(FileSystemRights.ReadPermissions, rule))
                                _allowReadPermissions = true;
                            if (contains(FileSystemRights.Synchronize, rule))
                                _allowSynchronize = true;
                            if (contains(FileSystemRights.TakeOwnership, rule))
                                _allowTakeOwnership = true;
                            if (contains(FileSystemRights.Traverse, rule))
                                _allowTraverse = true;
                            if (contains(FileSystemRights.Write, rule))
                                _allowWrite = true;
                            if (contains(FileSystemRights.WriteAttributes, rule))
                                _allowWriteAttributes = true;
                            if (contains(FileSystemRights.WriteData, rule))
                                _allowWriteData = true;
                            if (contains(FileSystemRights.WriteExtendedAttributes, rule))
                                _allowWriteExtendedAttributes = true;
                        }
                    }
                }

                IdentityReferenceCollection groups = _principal.Groups;
                for (int j = 0; j < groups.Count; j++)
                {
                    for (int i = 0; i < acl.Count; i++)
                    {
                        var rule =
                            (FileSystemAccessRule) acl[i];
                        if (groups[j].Equals(rule.IdentityReference))
                        {
                            if (AccessControlType.
                                Deny.Equals(rule.AccessControlType))
                            {
                                if (contains(FileSystemRights.AppendData, rule))
                                    _denyAppendData = true;
                                if (contains(FileSystemRights.ChangePermissions, rule))
                                    _denyChangePermissions = true;
                                if (contains(FileSystemRights.CreateDirectories, rule))
                                    _denyCreateDirectories = true;
                                if (contains(FileSystemRights.CreateFiles, rule))
                                    _denyCreateFiles = true;
                                if (contains(FileSystemRights.Delete, rule))
                                    _denyDelete = true;
                                if (contains(FileSystemRights.
                                    DeleteSubdirectoriesAndFiles, rule))
                                    _denyDeleteSubdirectoriesAndFiles = true;
                                if (contains(FileSystemRights.ExecuteFile, rule))
                                    _denyExecuteFile = true;
                                if (contains(FileSystemRights.FullControl, rule))
                                    _denyFullControl = true;
                                if (contains(FileSystemRights.ListDirectory, rule))
                                    _denyListDirectory = true;
                                if (contains(FileSystemRights.Modify, rule))
                                    _denyModify = true;
                                if (contains(FileSystemRights.Read, rule))
                                    _denyRead = true;
                                if (contains(FileSystemRights.ReadAndExecute, rule))
                                    _denyReadAndExecute = true;
                                if (contains(FileSystemRights.ReadAttributes, rule))
                                    _denyReadAttributes = true;
                                if (contains(FileSystemRights.ReadData, rule))
                                    _denyReadData = true;
                                if (contains(FileSystemRights.
                                    ReadExtendedAttributes, rule))
                                    _denyReadExtendedAttributes = true;
                                if (contains(FileSystemRights.ReadPermissions, rule))
                                    _denyReadPermissions = true;
                                if (contains(FileSystemRights.Synchronize, rule))
                                    _denySynchronize = true;
                                if (contains(FileSystemRights.TakeOwnership, rule))
                                    _denyTakeOwnership = true;
                                if (contains(FileSystemRights.Traverse, rule))
                                    _denyTraverse = true;
                                if (contains(FileSystemRights.Write, rule))
                                    _denyWrite = true;
                                if (contains(FileSystemRights.WriteAttributes, rule))
                                    _denyWriteAttributes = true;
                                if (contains(FileSystemRights.WriteData, rule))
                                    _denyWriteData = true;
                                if (contains(FileSystemRights.
                                    WriteExtendedAttributes, rule))
                                    _denyWriteExtendedAttributes = true;
                            }
                            else if (AccessControlType.
                                Allow.Equals(rule.AccessControlType))
                            {
                                if (contains(FileSystemRights.AppendData, rule))
                                    _allowAppendData = true;
                                if (contains(FileSystemRights.ChangePermissions, rule))
                                    _allowChangePermissions = true;
                                if (contains(FileSystemRights.CreateDirectories, rule))
                                    _allowCreateDirectories = true;
                                if (contains(FileSystemRights.CreateFiles, rule))
                                    _allowCreateFiles = true;
                                if (contains(FileSystemRights.Delete, rule))
                                    _allowDelete = true;
                                if (contains(FileSystemRights.
                                    DeleteSubdirectoriesAndFiles, rule))
                                    _allowDeleteSubdirectoriesAndFiles = true;
                                if (contains(FileSystemRights.ExecuteFile, rule))
                                    _allowExecuteFile = true;
                                if (contains(FileSystemRights.FullControl, rule))
                                    _allowFullControl = true;
                                if (contains(FileSystemRights.ListDirectory, rule))
                                    _allowListDirectory = true;
                                if (contains(FileSystemRights.Modify, rule))
                                    _allowModify = true;
                                if (contains(FileSystemRights.Read, rule))
                                    _allowRead = true;
                                if (contains(FileSystemRights.ReadAndExecute, rule))
                                    _allowReadAndExecute = true;
                                if (contains(FileSystemRights.ReadAttributes, rule))
                                    _allowReadAttributes = true;
                                if (contains(FileSystemRights.ReadData, rule))
                                    _allowReadData = true;
                                if (contains(FileSystemRights.
                                    ReadExtendedAttributes, rule))
                                    _allowReadExtendedAttributes = true;
                                if (contains(FileSystemRights.ReadPermissions, rule))
                                    _allowReadPermissions = true;
                                if (contains(FileSystemRights.Synchronize, rule))
                                    _allowSynchronize = true;
                                if (contains(FileSystemRights.TakeOwnership, rule))
                                    _allowTakeOwnership = true;
                                if (contains(FileSystemRights.Traverse, rule))
                                    _allowTraverse = true;
                                if (contains(FileSystemRights.Write, rule))
                                    _allowWrite = true;
                                if (contains(FileSystemRights.WriteAttributes, rule))
                                    _allowWriteAttributes = true;
                                if (contains(FileSystemRights.WriteData, rule))
                                    _allowWriteData = true;
                                if (contains(FileSystemRights.WriteExtendedAttributes,
                                    rule)) _allowWriteExtendedAttributes = true;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                //Deal with IO exceptions if you want
                throw e;
            }
        }

        public bool canAppendData()
        {
            return !_denyAppendData && _allowAppendData;
        }

        public bool canChangePermissions()
        {
            return !_denyChangePermissions && _allowChangePermissions;
        }

        public bool canCreateDirectories()
        {
            return !_denyCreateDirectories && _allowCreateDirectories;
        }

        public bool canCreateFiles()
        {
            return !_denyCreateFiles && _allowCreateFiles;
        }

        public bool canDelete()
        {
            return !_denyDelete && _allowDelete;
        }

        public bool canDeleteSubdirectoriesAndFiles()
        {
            return !_denyDeleteSubdirectoriesAndFiles &&
                   _allowDeleteSubdirectoriesAndFiles;
        }

        public bool canExecuteFile()
        {
            return !_denyExecuteFile && _allowExecuteFile;
        }

        public bool canFullControl()
        {
            return !_denyFullControl && _allowFullControl;
        }

        public bool canListDirectory()
        {
            return !_denyListDirectory && _allowListDirectory;
        }

        public bool canModify()
        {
            return !_denyModify && _allowModify;
        }

        public bool canRead()
        {
            return !_denyRead && _allowRead;
        }

        public bool canReadAndExecute()
        {
            return !_denyReadAndExecute && _allowReadAndExecute;
        }

        public bool canReadAttributes()
        {
            return !_denyReadAttributes && _allowReadAttributes;
        }

        public bool canReadData()
        {
            return !_denyReadData && _allowReadData;
        }

        public bool canReadExtendedAttributes()
        {
            return !_denyReadExtendedAttributes &&
                   _allowReadExtendedAttributes;
        }

        public bool canReadPermissions()
        {
            return !_denyReadPermissions && _allowReadPermissions;
        }

        public bool canSynchronize()
        {
            return !_denySynchronize && _allowSynchronize;
        }

        public bool canTakeOwnership()
        {
            return !_denyTakeOwnership && _allowTakeOwnership;
        }

        public bool canTraverse()
        {
            return !_denyTraverse && _allowTraverse;
        }

        public bool canWrite()
        {
            return !_denyWrite && _allowWrite;
        }

        public bool canWriteAttributes()
        {
            return !_denyWriteAttributes && _allowWriteAttributes;
        }

        public bool canWriteData()
        {
            return !_denyWriteData && _allowWriteData;
        }

        public bool canWriteExtendedAttributes()
        {
            return !_denyWriteExtendedAttributes &&
                   _allowWriteExtendedAttributes;
        }

        /// <span class="code-SummaryComment">
        ///     <summary>
        /// </span>
        /// Simple accessor
        /// <span class="code-SummaryComment"></summary></span>
        /// <span class="code-SummaryComment">
        ///     <returns></returns>
        /// </span>
        public WindowsIdentity getWindowsIdentity()
        {
            return _principal;
        }

        /// <span class="code-SummaryComment">
        ///     <summary>
        /// </span>
        /// Simple accessor
        /// <span class="code-SummaryComment"></summary></span>
        /// <span class="code-SummaryComment">
        ///     <returns></returns>
        /// </span>
        public String getPath()
        {
            return _path;
        }

        /// <span class="code-SummaryComment">
        ///     <summary>
        /// </span>
        /// Simply displays all allowed rights
        /// 
        /// Useful if say you want to test for write access and find
        /// it is false;
        /// <span class="code-SummaryComment">
        ///     <xmp>
        /// </span>
        /// UserFileAccessRights rights = new UserFileAccessRights(txtLogPath.Text);
        /// System.IO.FileInfo fi = new System.IO.FileInfo(txtLogPath.Text);
        /// if (rights.canWrite() && rights.canRead()) {
        /// lblLogMsg.Text = "R/W access";
        /// } else {
        /// if (rights.canWrite()) {
        /// lblLogMsg.Text = "Only Write access";
        /// } else if (rights.canRead()) {
        /// lblLogMsg.Text = "Only Read access";
        /// } else {
        /// lblLogMsg.CssClass = "error";
        /// lblLogMsg.Text = rights.ToString()
        /// }
        /// }
        /// <span class="code-SummaryComment"></xmp></span>
        /// <span class="code-SummaryComment"></summary></span>
        /// <span class="code-SummaryComment">
        ///     <returns></returns>
        /// </span>
        public override String ToString()
        {
            string str = "";

            if (canAppendData())
            {
                if (!String.IsNullOrEmpty(str))
                    str +=
                        ",";
                str += "AppendData";
            }
            if (canChangePermissions())
            {
                if (!String.IsNullOrEmpty(str))
                    str +=
                        ",";
                str += "ChangePermissions";
            }
            if (canCreateDirectories())
            {
                if (!String.IsNullOrEmpty(str))
                    str +=
                        ",";
                str += "CreateDirectories";
            }
            if (canCreateFiles())
            {
                if (!String.IsNullOrEmpty(str))
                    str +=
                        ",";
                str += "CreateFiles";
            }
            if (canDelete())
            {
                if (!String.IsNullOrEmpty(str))
                    str +=
                        ",";
                str += "Delete";
            }
            if (canDeleteSubdirectoriesAndFiles())
            {
                if (!String.IsNullOrEmpty(str))
                    str += ",";
                str += "DeleteSubdirectoriesAndFiles";
            }
            if (canExecuteFile())
            {
                if (!String.IsNullOrEmpty(str))
                    str += ",";
                str += "ExecuteFile";
            }
            if (canFullControl())
            {
                if (!String.IsNullOrEmpty(str))
                    str += ",";
                str += "FullControl";
            }
            if (canListDirectory())
            {
                if (!String.IsNullOrEmpty(str))
                    str += ",";
                str += "ListDirectory";
            }
            if (canModify())
            {
                if (!String.IsNullOrEmpty(str))
                    str += ",";
                str += "Modify";
            }
            if (canRead())
            {
                if (!String.IsNullOrEmpty(str))
                    str += ",";
                str += "Read";
            }
            if (canReadAndExecute())
            {
                if (!String.IsNullOrEmpty(str))
                    str += ",";
                str += "ReadAndExecute";
            }
            if (canReadAttributes())
            {
                if (!String.IsNullOrEmpty(str))
                    str += ",";
                str += "ReadAttributes";
            }
            if (canReadData())
            {
                if (!String.IsNullOrEmpty(str))
                    str += ",";
                str += "ReadData";
            }
            if (canReadExtendedAttributes())
            {
                if (!String.IsNullOrEmpty(str))
                    str += ",";
                str += "ReadExtendedAttributes";
            }
            if (canReadPermissions())
            {
                if (!String.IsNullOrEmpty(str))
                    str += ",";
                str += "ReadPermissions";
            }
            if (canSynchronize())
            {
                if (!String.IsNullOrEmpty(str))
                    str += ",";
                str += "Synchronize";
            }
            if (canTakeOwnership())
            {
                if (!String.IsNullOrEmpty(str))
                    str += ",";
                str += "TakeOwnership";
            }
            if (canTraverse())
            {
                if (!String.IsNullOrEmpty(str))
                    str += ",";
                str += "Traverse";
            }
            if (canWrite())
            {
                if (!String.IsNullOrEmpty(str))
                    str += ",";
                str += "Write";
            }
            if (canWriteAttributes())
            {
                if (!String.IsNullOrEmpty(str))
                    str += ",";
                str += "WriteAttributes";
            }
            if (canWriteData())
            {
                if (!String.IsNullOrEmpty(str))
                    str += ",";
                str += "WriteData";
            }
            if (canWriteExtendedAttributes())
            {
                if (!String.IsNullOrEmpty(str))
                    str += ",";
                str += "WriteExtendedAttributes";
            }
            if (String.IsNullOrEmpty(str))
                str = "None";
            return str;
        }

        /// <span class="code-SummaryComment">
        ///     <summary>
        /// </span>
        /// Convenience method to test if the right exists within the given rights
        /// <span class="code-SummaryComment"></summary></span>
        /// <span class="code-SummaryComment">
        ///     <param name="right"></param>
        /// </span>
        /// <span class="code-SummaryComment">
        ///     <param name="rule"></param>
        /// </span>
        /// <span class="code-SummaryComment">
        ///     <returns></returns>
        /// </span>
        public bool contains(FileSystemRights right,
            FileSystemAccessRule rule)
        {
            return (((int) right & (int) rule.FileSystemRights) == (int) right);
        }
    }
}